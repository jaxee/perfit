using System;
using UnityEngine;

namespace Obi
{
	[RequireComponent(typeof(ObiClothPicker))]
	public class ObiClothDragger : MonoBehaviour
	{
		private ObiClothPicker picker;
		private float originalMass = 0;
		private const float DRAGGED_PARTICLE_MASS = 0.0001f;

		void OnEnable()
		{
			picker = GetComponent<ObiClothPicker>();
			picker.OnParticlePicked += Picker_OnParticlePicked;
			picker.OnParticleHeld += Picker_OnParticleHeld;
			picker.OnParticleDragged += Picker_OnParticleDragged;
			picker.OnParticleReleased += Picker_OnParticleReleased;
		}

		void OnDisable()
		{
			picker.OnParticlePicked -= Picker_OnParticlePicked;
			picker.OnParticleHeld -= Picker_OnParticleHeld;
			picker.OnParticleDragged -= Picker_OnParticleDragged;
			picker.OnParticleReleased -= Picker_OnParticleReleased;
		}

		void Picker_OnParticleReleased (object sender, ObiClothPicker.ParticlePickEventArgs e)
		{
			Oni.SetParticleInverseMasses(picker.Cloth.Solver.OniSolver,new float[]{originalMass},1,picker.Cloth.particleIndices[e.particleIndex]);
			picker.Cloth.Solver.RelinquishRenderablePositions();
		}

		void Picker_OnParticleHeld (object sender, ObiClothPicker.ParticlePickEventArgs e)
		{
			// counteract gravity:
			Oni.SetParticleVelocities(picker.Cloth.Solver.OniSolver,new Vector4[]{-picker.Cloth.Solver.parameters.gravity * Time.fixedDeltaTime},1,picker.Cloth.particleIndices[e.particleIndex]);
		}

		void Picker_OnParticleDragged (object sender, ObiClothPicker.ParticlePickEventArgs e)
		{
			if (originalMass > 0){

				ObiSolver solver = picker.Cloth.Solver;

				Vector4 newPosition = solver.renderablePositions[picker.Cloth.particleIndices[e.particleIndex]] + (Vector4)e.mouseDelta;

				// convert new position to solver space if needed:
				if (solver.simulateInLocalSpace)
					newPosition = solver.transform.InverseTransformPoint(newPosition);
	
				Oni.SetParticlePositions(solver.OniSolver,new Vector4[]{newPosition},1,picker.Cloth.particleIndices[e.particleIndex]);

				// counteract gravity:
				Oni.SetParticleVelocities(solver.OniSolver,new Vector4[]{-solver.parameters.gravity * Time.fixedDeltaTime},1,picker.Cloth.particleIndices[e.particleIndex]);
			}
		}

		void Picker_OnParticlePicked (object sender, ObiClothPicker.ParticlePickEventArgs e)
		{
			picker.Cloth.Solver.RequireRenderablePositions();
			originalMass = picker.Cloth.invMasses[e.particleIndex];
			Oni.SetParticleInverseMasses(picker.Cloth.Solver.OniSolver,new float[]{DRAGGED_PARTICLE_MASS},1,picker.Cloth.particleIndices[e.particleIndex]);
		}
	}
}

