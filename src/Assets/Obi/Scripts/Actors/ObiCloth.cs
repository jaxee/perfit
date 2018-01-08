using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Obi
{

/**
 * An ObiCloth component generates a particle-based physical representation of the object geometry
 * to be feeded to an ObiSolver component. To do that, it needs connectivity information about the mesh,
 * which is provided by an ObiMeshConnectivity asset.
 * 
 * You can use it to make flags, capes, jackets, pants, ropes, drapes, nets, or any kind of object that exhibits cloth-like behavior.
 * 
 * ObiCloth objects have their particle properties expressed in local space. That means that particle positions, velocities, etc
 * are all expressed and serialized using the object's transform as reference. Thanks to this it is very easy to instantiate cloth prefabs and move/rotate/scale
 * them around, while keeping things working as expected. 
 * 
 * For convenience, solver gravity is expressed and applied in the simulation space used.
 */
[ExecuteInEditMode]
[AddComponentMenu("Physics/Obi/Obi Cloth")]
[RequireComponent(typeof (ObiSkinConstraints))]
[RequireComponent(typeof (ObiVolumeConstraints))]
[RequireComponent(typeof (ObiTetherConstraints))]
[DisallowMultipleComponent]
public class ObiCloth : ObiClothBase
{

	protected SkinnedMeshRenderer skinnedMeshRenderer;
	protected ObiAnimatorController animatorController;
	protected int rootBindPoseIndex = 0;

	public ObiSkinConstraints SkinConstraints{
		get{return constraints[Oni.ConstraintType.Skin] as ObiSkinConstraints;}
	}
	public ObiVolumeConstraints VolumeConstraints{
		get{return constraints[Oni.ConstraintType.Volume] as ObiVolumeConstraints;}
	}
	public ObiTetherConstraints TetherConstraints{
		get{return constraints[Oni.ConstraintType.Tether] as ObiTetherConstraints;}
	}

	public bool IsSkinned{
		get{return skinnedMeshRenderer != null && skinnedMeshRenderer.rootBone != null;}
	}
	
	public override Matrix4x4 ActorLocalToWorldMatrix{
		get{
			if (!IsSkinned) 
				return transform.localToWorldMatrix;
			else{
				if (!InSolver) 
					// when we are in edit mode, we need to take the root bone bind pose into account, as skinning is not updated by Obi but by Unity:
					return skinnedMeshRenderer.rootBone.localToWorldMatrix * skinnedMeshRenderer.sharedMesh.bindposes[rootBindPoseIndex];
				else 
					return skinnedMeshRenderer.rootBone.localToWorldMatrix;
			}
		}
	}

	public override Matrix4x4 ActorWorldToLocalMatrix{
		get{
			if (!IsSkinned) 
				return transform.worldToLocalMatrix;
			else{ 
				return skinnedMeshRenderer.rootBone.worldToLocalMatrix;
			}
		}
	}

	public override void Awake(){
		base.Awake();
		skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

		FindRootboneBindpose();
		SetupAnimatorController();
	}

	/**
	 * Finds the index of the rootbone bind pose in the sharedMesh's bindPose array. This is used
	 * to calculate actor world to local and local to world matrices when using skinned cloth.
     */
	private void FindRootboneBindpose(){

		if (IsSkinned){

			for(int i = 0; i < skinnedMeshRenderer.bones.Length; ++i){
				if (skinnedMeshRenderer.bones[i] == skinnedMeshRenderer.rootBone){
					rootBindPoseIndex = i;	
					return;
				}
			}
		}
	}

	/**
 	 * Find the first animator up the hierarchy of the cloth, and get its ObiAnimatorController component or add one if it is not present.
     */
	private void SetupAnimatorController(){

		if (IsSkinned){

			// find the first animator up our hierarchy:
			Animator animator = GetComponentInParent<Animator>();
				
			// if we have an animator above us, see if it has an animator controller component and add one if it doesn't:
			if (animator != null){

				animatorController = animator.GetComponent<ObiAnimatorController>();

				if (animatorController == null)
					animatorController = animator.gameObject.AddComponent<ObiAnimatorController>();
			}
		}		
	}

	public override void OnEnable(){

		base.OnEnable();

		// Initialize cloth:
		if (skinnedMeshRenderer == null)
			InitializeWithRegularMesh();
		else 
			InitializeWithSkinnedMesh();

	}
		
	public override void OnDisable(){
		
		base.OnDisable();

		if (skinnedMeshRenderer != null)
			skinnedMeshRenderer.sharedMesh = sharedMesh;

	}
	
	public override bool AddToSolver(object info){

		if (Initialized && base.AddToSolver(info)){
				
			particleIndicesHandle = Oni.PinMemory(particleIndices);

			Matrix4x4 w2lTransform = ActorWorldToLocalMatrix;

			// if solver data is expressed in local space, convert 
			// from solver's local space to world, then from world to actor local space:
			if (Solver.simulateInLocalSpace)
				w2lTransform *= Solver.transform.localToWorldMatrix;
	
			for (int i = 0; i < 16; ++i)
				transformData[i] = w2lTransform[i];

			IntPtr skinbatch = IntPtr.Zero;
			if (SkinConstraints.GetBatches().Count > 0)
				skinbatch = SkinConstraints.GetBatches()[0].OniBatch;

			deformableMesh = Oni.CreateDeformableMesh(Solver.OniSolver,
													  topology.HalfEdgeMesh,
													  skinbatch,
													  transformData,
													  particleIndicesHandle.AddrOfPinnedObject(),
												      sharedMesh.vertexCount,
													  sharedMesh.vertexCount);

			Oni.SetDeformableMeshTBNUpdate(deformableMesh,normalsUpdate,updateTangents);

			GetMeshDataArrays(clothMesh);

			SetSkinnedMeshAnimationInfo();

			GrabSkeletonBones();

			// Inits skeletal skinning, to ensure all particles are skinned during the first frame. This ensures
			// that the initial position of particles is the initial skinned position, instead of that dictated by the rootbone's local space.
			Oni.ForceDeformableMeshSkeletalSkinning(deformableMesh);

			CallOnDeformableMeshSetup();

			// remove bone weights so that the mesh is not affected by Unity's skinning:
			clothMesh.boneWeights = new BoneWeight[]{};

			return true;
		}
		return false;
    }

	public override bool RemoveFromSolver(object info){

		bool removed = false;

		try{

			// re-enable Unity skinning:
			if (clothMesh != null)
				clothMesh.boneWeights = sharedMesh.boneWeights;

			if (solver != null && InSolver){
				Oni.DestroyDeformableMesh(Solver.OniSolver,deformableMesh);
				deformableMesh = IntPtr.Zero;
			}

			Oni.UnpinMemory(particleIndicesHandle);
			Oni.UnpinMemory(meshTrianglesHandle);
			Oni.UnpinMemory(meshVerticesHandle);
			Oni.UnpinMemory(meshNormalsHandle);
			Oni.UnpinMemory(meshTangentsHandle);

			CallOnDeformableMeshTearDown();

		}catch(Exception e){
			Debug.LogException(e);
		}finally{
			removed = base.RemoveFromSolver(info);
		}
		return removed;
	}

	protected void SetSkinnedMeshAnimationInfo(){

		if (skinnedMeshRenderer != null){

			Matrix4x4[] rendererBindPoses = sharedMesh.bindposes;
			BoneWeight[] rendererWeights = sharedMesh.boneWeights;

			float[] bindPoses = new float[16*rendererBindPoses.Length];
			
			for (int p = 0; p < rendererBindPoses.Length; ++p){
				for (int i = 0; i < 16; ++i)
					bindPoses[p*16+i] = rendererBindPoses[p][i];
			}

			Oni.BoneWeights[] weights = new Oni.BoneWeights[rendererWeights.Length];
			for (int i = 0; i < rendererWeights.Length; ++i)
				weights[i] = new Oni.BoneWeights(rendererWeights[i]);

			Oni.SetDeformableMeshAnimationData(deformableMesh,bindPoses,weights,rendererBindPoses.Length);
		}
	}	

	protected void InitializeWithSkinnedMesh(){
	
        if (!Initialized)
           return;

        sharedMesh = sharedTopology.InputMesh;

		// Use the topology mesh as the shared mesh:
		skinnedMeshRenderer.sharedMesh = sharedMesh;
		
		// Make a deep copy of the original shared mesh.
		clothMesh = Mesh.Instantiate(sharedMesh) as Mesh;
		
		// remove bone weights so that the mesh is not affected by Unity's skinning:
		if (Application.isPlaying) 
			clothMesh.boneWeights = new BoneWeight[]{};

		clothMesh.MarkDynamic();
		GetMeshDataArrays(clothMesh);
		
		// Use the freshly created mesh copy as the renderer mesh:
		skinnedMeshRenderer.sharedMesh = clothMesh;

	}

	/**
	 * Generates the particle based physical representation of the cloth mesh. This is the initialization method for the cloth object
	 * and should not be called directly once the object has been created.
	 */
	public override IEnumerator GeneratePhysicRepresentationForMesh()
	{		
		initialized = false;
		initializing = false;
		
		if (sharedTopology == null){
			Debug.LogError("No ObiMeshTopology provided. Cannot initialize physical representation.");
			yield break;
		}else if (!sharedTopology.Initialized){
			Debug.LogError("The provided ObiMeshTopology contains no data. Cannot initialize physical representation.");
            yield break;
		}
		
		initializing = true;

		RemoveFromSolver(null);

		GameObject.DestroyImmediate(topology);
		topology = GameObject.Instantiate(sharedTopology);

		active = new bool[topology.heVertices.Length];
		positions = new Vector3[topology.heVertices.Length];
		restPositions = new Vector4[topology.heVertices.Length];
		velocities = new Vector3[topology.heVertices.Length];
		invMasses  = new float[topology.heVertices.Length];
		solidRadii = new float[topology.heVertices.Length];
		phases = new int[topology.heVertices.Length];
		areaContribution = new float[topology.heVertices.Length]; 
		deformableTriangles = new int[topology.heFaces.Length*3]; 

		// Create a particle for each vertex:
		for (int i = 0; i < topology.heVertices.Length; i++){
			
			Oni.Vertex vertex = topology.heVertices[i];

			// Get the particle's area contribution.
			areaContribution[i] = 0;
			foreach (Oni.Face face in topology.GetNeighbourFacesEnumerator(vertex)){
				areaContribution[i] += topology.GetFaceArea(face)/3;
            }
			
			// Get the shortest neighbour edge, particle radius will be half of its length.
			float minEdgeLength = Single.MaxValue;
			foreach (Oni.HalfEdge edge in topology.GetNeighbourEdgesEnumerator(vertex)){
				minEdgeLength = Mathf.Min(minEdgeLength,Vector3.Distance(topology.heVertices[topology.GetHalfEdgeStartVertex(edge)].position,
					                                                     topology.heVertices[edge.endVertex].position));
			}

			active[i] = true;
			invMasses[i] = (skinnedMeshRenderer == null && areaContribution[i] > 0) ? (1.0f / (DEFAULT_PARTICLE_MASS * areaContribution[i])) : 0;
			positions[i] = vertex.position ;
			restPositions[i] = positions[i];
			restPositions[i][3] = 0; // activate rest position.
			solidRadii[i] = minEdgeLength * 0.5f;
			phases[i] = Oni.MakePhase(1,selfCollisions?Oni.ParticlePhase.SelfCollide:0);
			
			if (i % 500 == 0)
				yield return new CoroutineJob.ProgressInfo("ObiCloth: generating particles...",i/(float)topology.heVertices.Length);
		}
		
		// Generate deformable triangles:
		for (int i = 0; i < topology.heFaces.Length; i++){

			Oni.Face face = topology.heFaces[i];
			
			Oni.HalfEdge e1 = topology.heHalfEdges[face.halfEdge];
			Oni.HalfEdge e2 = topology.heHalfEdges[e1.nextHalfEdge];
			Oni.HalfEdge e3 = topology.heHalfEdges[e2.nextHalfEdge];

			deformableTriangles[i*3] = e1.endVertex;
			deformableTriangles[i*3+1] = e2.endVertex;
			deformableTriangles[i*3+2] = e3.endVertex;

			if (i % 500 == 0)
				yield return new CoroutineJob.ProgressInfo("ObiCloth: generating deformable geometry...",i/(float)topology.heFaces.Length);
		}

		List<ObiMeshTopology.HEEdge> edges = topology.GetEdgeList();

		DistanceConstraints.Clear();
		ObiDistanceConstraintBatch distanceBatch = new ObiDistanceConstraintBatch(true,false);
		DistanceConstraints.AddBatch(distanceBatch);

		// Create distance springs: 
		for (int i = 0; i < edges.Count; i++){
		
			Oni.HalfEdge hedge = topology.heHalfEdges[edges[i].halfEdgeIndex];
			Oni.Vertex startVertex = topology.heVertices[topology.GetHalfEdgeStartVertex(hedge)];
			Oni.Vertex endVertex = topology.heVertices[hedge.endVertex];
			
			distanceBatch.AddConstraint(topology.GetHalfEdgeStartVertex(hedge),hedge.endVertex,Vector3.Distance(startVertex.position,endVertex.position),1,1);		

			if (i % 500 == 0)
				yield return new CoroutineJob.ProgressInfo("ObiCloth: generating structural constraints...",i/(float)topology.heHalfEdges.Length);
		}

		// Cook distance constraints, for better cache and SIMD use:
		distanceBatch.Cook();
		
		// Create aerodynamic constraints:
		AerodynamicConstraints.Clear();
		ObiAerodynamicConstraintBatch aeroBatch = new ObiAerodynamicConstraintBatch(false,false);
		AerodynamicConstraints.AddBatch(aeroBatch);

		for (int i = 0; i < topology.heVertices.Length; i++){

			aeroBatch.AddConstraint(i,
									areaContribution[i],
			                        AerodynamicConstraints.dragCoefficient,
			                        AerodynamicConstraints.liftCoefficient);

			if (i % 500 == 0)
				yield return new CoroutineJob.ProgressInfo("ObiCloth: generating aerodynamic constraints...",i/(float)topology.heFaces.Length);
		}

		//Create skin constraints (if needed)
		if (skinnedMeshRenderer != null){

			SkinConstraints.Clear();
			ObiSkinConstraintBatch skinBatch = new ObiSkinConstraintBatch(true,false,0,0.02f);
			SkinConstraints.AddBatch(skinBatch);

			for (int i = 0; i < topology.heVertices.Length; ++i){

				skinBatch.AddConstraint(i,topology.heVertices[i].position,
											   Vector3.up,0.05f,0.1f,0,1);

				if (i % 500 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiCloth: generating skin constraints...",i/(float)topology.heVertices.Length);
			}

			for (int i = 0; i < topology.normals.Length; ++i){
				skinBatch.skinNormals[topology.visualMap[i]] = topology.normals[i];
			}

			skinBatch.Cook();

		}

		//Create pressure constraints if the mesh is closed:
		VolumeConstraints.Clear();

		if (topology.IsClosed){

			ObiVolumeConstraintBatch volumeBatch = new ObiVolumeConstraintBatch(false,false);
			VolumeConstraints.AddBatch(volumeBatch);

			int[] triangleIndices = new int[topology.heFaces.Length * 3];
			for (int i = 0; i < topology.heFaces.Length; i++){
				Oni.Face face = topology.heFaces[i];
			
				Oni.HalfEdge e1 = topology.heHalfEdges[face.halfEdge];
				Oni.HalfEdge e2 = topology.heHalfEdges[e1.nextHalfEdge];
				Oni.HalfEdge e3 = topology.heHalfEdges[e2.nextHalfEdge];

				triangleIndices[i*3] = e1.endVertex;
				triangleIndices[i*3+1] = e2.endVertex;
				triangleIndices[i*3+2] = e3.endVertex;

				if (i % 500 == 0)
					yield return new CoroutineJob.ProgressInfo("ObiCloth: generating volume constraints...",i/(float)topology.heFaces.Length);
			}

			volumeBatch.AddConstraint(triangleIndices,topology.MeshVolume,1,1);
		}
		
		//Create bending constraints:
		BendingConstraints.Clear();
		ObiBendConstraintBatch bendBatch = new ObiBendConstraintBatch(true,false);
		BendingConstraints.AddBatch(bendBatch);

		Dictionary<int,int> cons = new Dictionary<int, int>();
		for (int i = 0; i < topology.heVertices.Length; i++){
	
			Oni.Vertex vertex = topology.heVertices[i];
	
			foreach (Oni.Vertex n1 in topology.GetNeighbourVerticesEnumerator(vertex)){
	
				float cosBest = 0;
				Oni.Vertex vBest = n1;
	
				foreach (Oni.Vertex n2 in topology.GetNeighbourVerticesEnumerator(vertex)){
					float cos = Vector3.Dot((n1.position-vertex.position).normalized,
					                        (n2.position-vertex.position).normalized);
					if (cos < cosBest){
						cosBest = cos;
						vBest = n2;
					}
				}
				
				if (!cons.ContainsKey(vBest.index) || cons[vBest.index] != n1.index){
				
					cons[n1.index] = vBest.index;
				
					float[] restPositions = new float[]{n1.position[0],n1.position[1],n1.position[2],
														vBest.position[0],vBest.position[1],vBest.position[2],
														vertex.position[0],vertex.position[1],vertex.position[2]};
					float restBend = Oni.BendingConstraintRest(restPositions);
					bendBatch.AddConstraint(n1.index,vBest.index,vertex.index,restBend,0,1);
				}
	
			}
	
			if (i % 500 == 0)
				yield return new CoroutineJob.ProgressInfo("ObiCloth: adding bend constraints...",i/(float)sharedTopology.heVertices.Length);
		}

		bendBatch.Cook();

		// Initialize tether constraints:
		TetherConstraints.Clear();

		// Initialize pin constraints:
		PinConstraints.Clear();
		ObiPinConstraintBatch pinBatch = new ObiPinConstraintBatch(false,false,0,0.02f);
		PinConstraints.AddBatch(pinBatch);

		initializing = false;
		initialized = true;

		if (skinnedMeshRenderer == null)
			InitializeWithRegularMesh();
		else 
			InitializeWithSkinnedMesh();
	}

	/**
	 * In the case of skinned cloth, we need to tell the animator controller to update the skeletal animation, 
	 * then grab the skinned vertex positions prior to starting the simulation steps for this frame.
	 */
	public override void OnSolverStepBegin(){
		if (skinnedMeshRenderer == null){
			// regular on step begin: transform fixed particles.
			base.OnSolverStepBegin();
		}else{

			// manually update animator (before particle physics):
			if (animatorController != null){
				if (solver.simulationOrder != ObiSolver.SimulationOrder.LateUpdate)
					animatorController.UpdateAnimation();
				else
					animatorController.ResumeAutonomousUpdate();
			}

			// apply world space velocity:
			ApplyWorldSpaceVelocity();

			// grab skeleton bone transforms:
			GrabSkeletonBones();
		}
	}

	public override void OnSolverStepEnd(){	

		base.OnSolverStepEnd();

		if (animatorController != null)
			animatorController.ResetUpdateFlag();

	}

	public void ApplyWorldSpaceVelocity(){

		if (!Solver.simulateInLocalSpace || worldVelocityScale == 0 || !this.enabled) 
			return;

		Matrix4x4 delta = Solver.transform.worldToLocalMatrix * Solver.LastTransform;
		Vector4[] simulationPosition = {Vector4.zero};
		Vector4 localPosition;

		float iScale = 1-Mathf.Clamp01(worldVelocityScale);

		for(int i = 0; i < particleIndices.Length; ++i){
			if (invMasses[i] > 0){

				Oni.GetParticlePositions(solver.OniSolver,simulationPosition,1,particleIndices[i]);

				simulationPosition[0].w = 1; 
				localPosition = delta * simulationPosition[0];
	
				// lerp between local and world positions:
				simulationPosition[0].Set(simulationPosition[0].x * iScale + localPosition.x * worldVelocityScale,
										  simulationPosition[0].y * iScale + localPosition.y * worldVelocityScale,
										  simulationPosition[0].z * iScale + localPosition.z * worldVelocityScale,0);

				Oni.SetParticlePositions(solver.OniSolver,simulationPosition,1,particleIndices[i]);

			}
		}
	}

	/**
	 * If a Skinned Mesh Renderer is present, grabs all mesh data from the current animation state and transfers it to the particle simulation.
	 * Does nothing if no Skinned Mesh Renderer can be found.
	 */
	public void GrabSkeletonBones(){

		if (skinnedMeshRenderer != null){

			if (!Initialized || clothMesh == null || particleIndices == null) return;

			Transform[] rendererBones = skinnedMeshRenderer.bones;
			float[] bones = new float[16*rendererBones.Length];
			
			for (int p = 0; p < sharedMesh.bindposes.Length; ++p){
	
				Matrix4x4 bone;

				if (Solver.simulateInLocalSpace)
					bone = Solver.transform.worldToLocalMatrix * rendererBones[p].localToWorldMatrix;
				else 
					bone = rendererBones[p].localToWorldMatrix;

				for (int i = 0; i < 16; ++i)
					bones[p*16+i] = bone[i];
			}

			Oni.SetDeformableMeshBoneTransforms(deformableMesh,bones);
		}

	}

	private List<HashSet<int>> GenerateIslands(IEnumerable<int> particles, bool onlyFixed){

		List<HashSet<int>> islands = new List<HashSet<int>>();
			
		// Partition fixed particles into islands:
		foreach (int i in particles){
			
			Oni.Vertex vertex = topology.heVertices[i];

			if ((onlyFixed && invMasses[i] > 0) || !active[i]) continue;
			
			int assignedIsland = -1;

			// keep a list of islands to merge with ours:
			List<int> mergeableIslands = new List<int>();
				
			// See if any of our neighbors is part of an island:
			foreach (Oni.Vertex n in topology.GetNeighbourVerticesEnumerator(vertex)){

				if (!active[n.index]) continue;
	
				for(int k = 0; k < islands.Count; ++k){

					if (islands[k].Contains(n.index)){

						// if we are not in an island yet, pick this one:
						if (assignedIsland < 0){
							assignedIsland = k;
                            islands[k].Add(i);
						}
						// if we already are in an island, we will merge this newfound island with ours:
						else if (assignedIsland != k && !mergeableIslands.Contains(k)){
							mergeableIslands.Add(k);
						}
					}
                }
			}

			// merge islands with the assigned one:
			foreach(int merge in mergeableIslands){
				islands[assignedIsland].UnionWith(islands[merge]);
			}

			// remove merged islands:
			mergeableIslands.Sort();
			mergeableIslands.Reverse();
			foreach(int merge in mergeableIslands){
				islands.RemoveAt(merge);
			}
			
			// If no adjacent particle is in an island, create a new one:
			if (assignedIsland < 0){
				islands.Add(new HashSet<int>(){i});
			}
		}	

		return islands;
	}

	/**
	 * This function generates tethers for a given set of particles, all belonging a connected graph. 
	 * This is use ful when the cloth mesh is composed of several
	 * disjoint islands, and we dont want tethers in one island to anchor particles to fixed particles in a different island.
	 * 
	 * Inside each island, fixed particles are partitioned again into "islands", then generates up to maxTethers constraints for each 
	 * particle linking it to the closest point in each fixed island.
	 */
	private void GenerateTethersForIsland(HashSet<int> particles, int maxTethers){

		if (maxTethers > 0){
			ObiTetherConstraintBatch tetherBatch = new ObiTetherConstraintBatch(true,false);
			TetherConstraints.AddBatch(tetherBatch);
			
			List<HashSet<int>> fixedIslands = GenerateIslands(particles,true);
			
			// Generate tether constraints:
			foreach (int i in particles){
			
				if (invMasses[i] == 0 || !active[i]) continue;
				
				List<KeyValuePair<float,int>> tethers = new List<KeyValuePair<float,int>>(fixedIslands.Count*maxTethers);
				
				// Find the closest particle in each island, and add it to tethers.
				foreach(HashSet<int> island in fixedIslands){
					int closest = -1;
					float minDistance = Mathf.Infinity;
					foreach (int j in island){
						float distance = (topology.heVertices[i].position - topology.heVertices[j].position).sqrMagnitude;
						if (distance < minDistance){
							minDistance = distance;
							closest = j;
						}
					}
					if (closest >= 0)
						tethers.Add(new KeyValuePair<float,int>(minDistance, closest));
				}
				
				// Sort tether indices by distance:
				tethers.Sort(
				delegate(KeyValuePair<float,int> x, KeyValuePair<float,int> y)
				{
					return x.Key.CompareTo(y.Key);
				}
				);
				
				// Create constraints for "maxTethers" closest anchor particles:
				for (int k = 0; k < Mathf.Min(maxTethers,tethers.Count); ++k){
					tetherBatch.AddConstraint(i,tethers[k].Value,Mathf.Sqrt(tethers[k].Key),
												TetherConstraints.tetherScale,
												TetherConstraints.stiffness);
				}
			}
	        
			tetherBatch.Cook();
		}
	}	

	/**
	 * Automatically generates tether constraints for the cloth.
	 * Partitions fixed particles into "islands", then generates up to maxTethers constraints for each 
	 * particle, linking it to the closest point in each island.
	 */
	public override bool GenerateTethers(TetherType type){
		
		if (!Initialized) return false;

		TetherConstraints.Clear();
		
		// generate disjoint islands:
		List<HashSet<int>> islands = GenerateIslands(System.Linq.Enumerable.Range(0, topology.heVertices.Length),false);

		// generate tethers for each one:
		foreach(HashSet<int> island in islands)
			GenerateTethersForIsland(island,4);
        
        return true;
        
	}
}
}

