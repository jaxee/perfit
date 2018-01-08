using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;


namespace Obi{
	
	/**
 * Custom inspector for ObiCloth components.
 * Allows particle selection and constraint edition. 
 * 
 * Selection:
 * 
 * - To select a particle, left-click on it. 
 * - You can select multiple particles by holding shift while clicking.
 * - To deselect all particles, click anywhere on the object except a particle.
 * 
 * Constraints:
 * 
 * - To edit particle constraints, select the particles you wish to edit.
 * - Constraints affecting any of the selected particles will appear in the inspector.
 * - To add a new pin constraint to the selected particle(s), click on "Add Pin Constraint".
 * 
 */
	[CustomEditor(typeof(ObiTearableCloth)), CanEditMultipleObjects] 
	public class ObiTearableClothEditor : ObiParticleActorEditor
	{

		public class TearableClothParticleProperty : ParticleProperty
		{
		  public const int TearResistance = 3;

		  public TearableClothParticleProperty (int value) : base (value){}
		}

		[MenuItem("GameObject/3D Object/Obi/Obi Tearable Cloth",false,2)]
		static void CreateObiCloth()
		{
			GameObject c = new GameObject("Obi Tearable Cloth");
			Undo.RegisterCreatedObjectUndo(c,"Create Obi Tearable Cloth");
			c.AddComponent<MeshFilter>();
			c.AddComponent<MeshRenderer>();
			c.AddComponent<ObiTearableCloth>();
		}

		[MenuItem("GameObject/3D Object/Obi/Obi Tearable Cloth (with solver)",false,3)]
		static void CreateObiClothWithSolver()
		{
			GameObject c = new GameObject("Obi Tearable Cloth");
			Undo.RegisterCreatedObjectUndo(c,"Create Obi Tearable Cloth");
			c.AddComponent<MeshFilter>();
			c.AddComponent<MeshRenderer>();
			ObiTearableCloth cloth = c.AddComponent<ObiTearableCloth>();
			ObiSolver solver = c.AddComponent<ObiSolver>();
			cloth.Solver = solver;
		}
		
		ObiTearableCloth cloth;
		
		public override void OnEnable(){

			base.OnEnable();
			cloth = (ObiTearableCloth)target;

			particlePropertyNames.AddRange(new string[]{"Tear Resistance"});

		}
		
		public override void OnDisable(){
			base.OnDisable();
			EditorUtility.ClearProgressBar();
		}

		public override void UpdateParticleEditorInformation(){
			
			for(int i = 0; i < cloth.positions.Length; i++)
			{
				wsPositions[i] = cloth.GetParticlePosition(i);		
			}

			if (cloth.clothMesh != null && Camera.current != null){
			
				for(int i = 0; i < cloth.clothMesh.vertexCount; i++){

					int particle = cloth.topology.visualMap[i];
					Vector3 camToParticle = Camera.current.transform.position - wsPositions[particle];

					sqrDistanceToCamera[particle] = camToParticle.sqrMagnitude;
					facingCamera[particle] = (Vector3.Dot(cloth.transform.TransformVector(cloth.MeshNormals[i]),camToParticle) > 0);
		
				}
			}
			
		}

		protected override void DrawActorInfo(){

			if (cloth.clothMesh == null)
				return;

			Material mat = Resources.Load<Material>("PropertyGradientMaterial");
			
			if (mat.SetPass(0)) {

				Mesh gradientMesh = GameObject.Instantiate(cloth.clothMesh);

				Color[] colors = new Color[gradientMesh.vertexCount];

				for(int i = 0; i < gradientMesh.vertexCount; i++){

					// get particle index for this vertex:
					int particle = cloth.topology.visualMap[i];

					// calculate vertex color:
					if (selectionMask && !selectionStatus[particle]){
						colors[i] = Color.black;
					}else{
						colors[i] = GetPropertyValueGradient(GetPropertyValue(currentProperty,particle));
					}

				}

				gradientMesh.colors = colors;

				Graphics.DrawMeshNow(gradientMesh,cloth.ActorLocalToWorldMatrix);
				
			}

			DrawParticleRadii();

			if (!paintBrush){
				DrawParticles();
			}
		
		}
		
		protected override void SetPropertyValue(ParticleProperty property,int index, float value){
			if (index >= 0 && index < cloth.invMasses.Length){
				switch(property){
				case TearableClothParticleProperty.Mass: 
						cloth.invMasses[index] = 1.0f / (Mathf.Max(value,0.00001f) * cloth.areaContribution[index]);
					break; 
				case TearableClothParticleProperty.Radius:
						cloth.solidRadii[index] = value;
					break;
				case ParticleProperty.Layer:
						cloth.phases[index] = Oni.MakePhase((int)value,cloth.SelfCollisions?Oni.ParticlePhase.SelfCollide:0);;
					break;
				case TearableClothParticleProperty.TearResistance:
					if (cloth is ObiTearableCloth)
						cloth.tearResistance[index] = value;
					break;
				}
			}
		}
		
		protected override float GetPropertyValue(ParticleProperty property, int index){
			if (index >= 0 && index < cloth.invMasses.Length){
				switch(property){
				case TearableClothParticleProperty.Mass:
					return 1.0f / (cloth.invMasses[index] * cloth.areaContribution[index]);
				case TearableClothParticleProperty.Radius:
					return cloth.solidRadii[index];
				case ParticleProperty.Layer:
					return Oni.GetGroupFromPhase(cloth.phases[index]);
				case TearableClothParticleProperty.TearResistance:
					return cloth.tearResistance[index];
				}
			}
			return 0;
		}

		public override void OnInspectorGUI() {
			
			serializedObject.UpdateIfRequiredOrScript();

			GUI.enabled = cloth.Initialized;
			EditorGUI.BeginChangeCheck();
			editMode = GUILayout.Toggle(editMode,new GUIContent("Edit particles",Resources.Load<Texture2D>("EditParticles")),"LargeButton");
			if (EditorGUI.EndChangeCheck()){
				SceneView.RepaintAll();
			}		
			GUI.enabled = true;	

			EditorGUILayout.LabelField("Status: "+ (cloth.Initialized ? "Initialized":"Not initialized"));

			GUI.enabled = (cloth.SharedTopology != null);
			if (GUILayout.Button("Initialize")){
				if (!cloth.Initialized){
					CoroutineJob job = new CoroutineJob();
					routine = EditorCoroutine.StartCoroutine(job.Start(cloth.GeneratePhysicRepresentationForMesh()));
					EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
				}else{
					if (EditorUtility.DisplayDialog("Actor initialization","Are you sure you want to re-initialize this actor?","Ok","Cancel")){
						CoroutineJob job = new CoroutineJob();
						routine = EditorCoroutine.StartCoroutine(job.Start(cloth.GeneratePhysicRepresentationForMesh()));
						EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
					}
				}
			}
			GUI.enabled = true;

			if (cloth.SharedTopology == null){
				EditorGUILayout.HelpBox("No ObiMeshTopology asset present.",MessageType.Info);
			}

			GUI.enabled = cloth.Initialized;
			if (GUILayout.Button("Set Rest State")){
				Undo.RecordObject(cloth, "Set rest state");
				cloth.PullDataFromSolver(ParticleData.POSITIONS | ParticleData.VELOCITIES);
			}
			GUI.enabled = true;

			EditorGUI.BeginChangeCheck();
			ObiSolver solver = EditorGUILayout.ObjectField("Solver",cloth.Solver, typeof(ObiSolver), true) as ObiSolver;
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(cloth, "Set solver");
				cloth.Solver = solver;
			}

			EditorGUI.BeginChangeCheck();
			ObiCollisionMaterial material = EditorGUILayout.ObjectField("Collision Material",cloth.CollisionMaterial, typeof(ObiCollisionMaterial), false) as ObiCollisionMaterial;
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(cloth, "Set collision material");
				cloth.CollisionMaterial = material;
			}

			EditorGUI.BeginChangeCheck();
			ObiMeshTopology topology = EditorGUILayout.ObjectField("Shared Topology",cloth.SharedTopology, typeof(ObiMeshTopology), true) as ObiMeshTopology;
			if (EditorGUI.EndChangeCheck()){
				Undo.RecordObject(cloth, "Set topology");
				cloth.SharedTopology  = topology;
			}

			bool newSelfCollisions = EditorGUILayout.Toggle(new GUIContent("Self collisions","Enabling this allows particles generated by this actor to interact with each other."),cloth.SelfCollisions);
			if (cloth.SelfCollisions != newSelfCollisions){
				Undo.RecordObject(cloth, "Set self collisions");
				cloth.SelfCollisions = newSelfCollisions;
			}

			bool newUpdateTangents = EditorGUILayout.Toggle(new GUIContent("Update tangents","If enabled, tangent space will be updated after each simulation step. Enable this if your cloth uses normal maps."),cloth.UpdateTangents);
			if (cloth.UpdateTangents != newUpdateTangents){
				Undo.RecordObject(cloth, "Set update tangents");
				cloth.UpdateTangents = newUpdateTangents;
			}

			Oni.NormalsUpdate newNormalsUpdate = (Oni.NormalsUpdate)EditorGUILayout.EnumPopup(new GUIContent("Update normals","If set to recalculate, smooth normals will be recalculated each step. If set to skin, the original mesh normals will be rotated based on surface orientation."),cloth.NormalsUpdate);
			if (cloth.NormalsUpdate != newNormalsUpdate){
				Undo.RecordObject(cloth, "Set normals update");
				cloth.NormalsUpdate = newNormalsUpdate;
			}

			Editor.DrawPropertiesExcluding(serializedObject,"m_Script");

			// Progress bar:
			EditorCoroutine.ShowCoroutineProgressBar("Generating physical representation...",routine);
			
			// Apply changes to the serializedProperty
			if (GUI.changed){
				serializedObject.ApplyModifiedProperties();
			}
			
		}
		
	}
}


