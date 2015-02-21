﻿using System;
using System.Drawing;
using SimpleScene.Util;
using OpenTK;
using OpenTK.Graphics;

namespace SimpleScene
{
    /// <summary>
    /// Emits particles on demand via EmitParticles(...) or periodically via Simulate(...)
    /// </summary>
    public abstract class SSParticleEmitter
    {
        public delegate void ReceiverHandler(SSParticle newParticle);

        protected readonly static SSParticle c_defaultParticle = new SSParticle();

        protected static Random s_rand = new Random ();

        public float EmissionDelay = 0f; // TODO

        public float EmissionIntervalMin = 1.0f;
        public float EmissionIntervalMax = 1.0f;
        public float EmissionInterval {
            set { EmissionIntervalMin = EmissionIntervalMax = value; }
        }

        public int ParticlesPerEmissionMin = 1;
        public int ParticlesPerEmissionMax = 1;
        public int ParticlesPerEmission {
            set { ParticlesPerEmissionMin = ParticlesPerEmissionMax = value; }
        }

        public float LifeMin = c_defaultParticle.Life;
        public float LifeMax = c_defaultParticle.Life;
        public float Life {
            set { LifeMin = LifeMax = value; }
        }

        public Vector3 VelocityComponentMin = c_defaultParticle.Vel;
        public Vector3 VelocityComponentMax = c_defaultParticle.Vel;
        public Vector3 Velocity {
            set { VelocityComponentMin = VelocityComponentMax = value; }
        }

        public Vector3 OrientationMin = c_defaultParticle.Orientation;
        public Vector3 OrientationMax = c_defaultParticle.Orientation;
        public Vector3 Orientation {
            set { OrientationMin = OrientationMax = value; }
        }

        public Vector3 AngularVelocityMin = c_defaultParticle.AngularVelocity;
        public Vector3 AngularVelocityMax = c_defaultParticle.AngularVelocity;
        public Vector3 AngularVelocity {
            set { AngularVelocityMin = AngularVelocityMax = value; }
        }

        public float MasterScaleMin = c_defaultParticle.MasterScale;
        public float MasterScaleMax = c_defaultParticle.MasterScale;
        public float MasterScale {
            set { MasterScaleMin = MasterScaleMax = value; }
        }

        public Vector3 ComponentScaleMin = c_defaultParticle.ComponentScale;
        public Vector3 ComponentScaleMax = c_defaultParticle.ComponentScale;
        public Vector3 ComponentScale {
            set { ComponentScaleMin = ComponentScaleMax = value; }
        }

        public Color4 ColorComponentMin = c_defaultParticle.Color;
        public Color4 ColorComponentMax = c_defaultParticle.Color;
        public Color4 Color {
            set { ColorComponentMin = ColorComponentMax = value; }
        }

        public RectangleF[] SpriteRectangles = { c_defaultParticle.SpriteRect };
        public byte[] SpriteIndices = { c_defaultParticle.SpriteIndex };

		public byte[] EffectorMasks = { c_defaultParticle.EffectorMask };

        private float m_initialDelay;
        private float m_timeSinceLastEmission;
        private float m_nextEmission;

        public SSParticleEmitter()
        {
            Reset();
        }

        public virtual void Reset()
        {
            m_initialDelay = EmissionDelay;
            m_timeSinceLastEmission = float.PositiveInfinity;
            m_nextEmission = 0f;
        }

        public void EmitParticles(ReceiverHandler receiver)
        {
            int numToEmit = s_rand.Next(ParticlesPerEmissionMin, ParticlesPerEmissionMax);
            emitParticles(numToEmit, receiver);
        }

        public void Simulate(float deltaT, ReceiverHandler receiver) 
        {
            if (m_initialDelay > 0f) {
                // if initial delay is needed
                m_initialDelay -= deltaT;
                if (m_initialDelay > 0f) {
                    return;
                }
            }

            m_timeSinceLastEmission += deltaT;
            if (m_timeSinceLastEmission > m_nextEmission) {
                EmitParticles(receiver);
                m_timeSinceLastEmission = 0f;
                m_nextEmission = Interpolate.Lerp(EmissionIntervalMin, EmissionIntervalMax, 
                    (float)s_rand.NextDouble());
            }
        }

        /// <summary>
        /// Convenience function.
        /// </summary>
        static protected float nextFloat()
        {
            return (float)s_rand.NextDouble();
        }

        /// <summary>
        /// Override by the derived classes to describe how new particles are emitted
        /// </summary>
        /// <param name="particleCount">Particle count.</param>
        /// <param name="receiver">Receiver.</param>
        protected abstract void emitParticles (int particleCount, ReceiverHandler receiver);

        /// <summary>
        /// To be used by derived classes for shared particle setup
        /// </summary>
        /// <param name="p">particle to setup</param>
        protected virtual void configureNewParticle(SSParticle p)
        {
            p.Life = Interpolate.Lerp(LifeMin, LifeMax, nextFloat());

            p.ComponentScale.X = Interpolate.Lerp(ComponentScaleMin.X, ComponentScaleMax.X, nextFloat());
            p.ComponentScale.Y = Interpolate.Lerp(ComponentScaleMin.Y, ComponentScaleMax.Y, nextFloat());
            p.ComponentScale.Z = Interpolate.Lerp(ComponentScaleMin.Z, ComponentScaleMax.Z, nextFloat());

            p.Orientation.X = Interpolate.Lerp(OrientationMin.X, OrientationMax.X, nextFloat());
            p.Orientation.Y = Interpolate.Lerp(OrientationMin.Y, OrientationMax.Y, nextFloat());
            p.Orientation.Z = Interpolate.Lerp(OrientationMin.Z, OrientationMax.Z, nextFloat());

            p.AngularVelocity.X = Interpolate.Lerp(AngularVelocityMin.X, AngularVelocityMax.X, nextFloat());
            p.AngularVelocity.Y = Interpolate.Lerp(AngularVelocityMin.Y, AngularVelocityMax.Y, nextFloat());
            p.AngularVelocity.Z = Interpolate.Lerp(AngularVelocityMin.Z, AngularVelocityMax.Z, nextFloat());

            p.Vel.X = Interpolate.Lerp(VelocityComponentMin.X, VelocityComponentMax.X, nextFloat());
            p.Vel.Y = Interpolate.Lerp(VelocityComponentMin.Y, VelocityComponentMax.Y, nextFloat());
            p.Vel.Z = Interpolate.Lerp(VelocityComponentMin.Z, VelocityComponentMax.Z, nextFloat());

            p.MasterScale = Interpolate.Lerp(MasterScaleMin, MasterScaleMax, nextFloat());

            p.Color.R = Interpolate.Lerp(ColorComponentMin.R, ColorComponentMax.R, nextFloat());
            p.Color.G = Interpolate.Lerp(ColorComponentMin.G, ColorComponentMax.G, nextFloat());
            p.Color.B = Interpolate.Lerp(ColorComponentMin.B, ColorComponentMax.B, nextFloat());
            p.Color.A = Interpolate.Lerp(ColorComponentMin.A, ColorComponentMax.A, nextFloat());

            p.SpriteIndex = SpriteIndices [s_rand.Next(0, SpriteIndices.Length - 1)];
            p.SpriteRect = SpriteRectangles [s_rand.Next(0, SpriteRectangles.Length - 1)];
        }
    }

    /// <summary>
    /// Emits via an instance of ParticlesFieldGenerator
    /// </summary>
    public class SSParticlesFieldEmitter : SSParticleEmitter
    {
        protected ParticlesFieldGenerator m_fieldGenerator;

        public SSParticlesFieldEmitter(ParticlesFieldGenerator fieldGenerator)
        {
            m_fieldGenerator = fieldGenerator;
        }

        public void SetSeed(int seed)
        {
            m_fieldGenerator.SetSeed(seed);
        }

        protected override void emitParticles (int particleCount, ReceiverHandler particleReceiver)
        {
            SSParticle newParticle = new SSParticle();
            ParticlesFieldGenerator.NewParticleDelegate fieldReceiver = (id, pos) => {
                configureNewParticle(newParticle);
                newParticle.Pos = pos;
                particleReceiver(newParticle);
                return true;
            };
            m_fieldGenerator.Generate(particleCount, fieldReceiver);
        }
    }
}

