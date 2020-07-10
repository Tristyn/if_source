using UnityEngine;

/******************************************************************************
  Simple harmonic motion functions. Modified to work with Unity. Below is the original notice.


  Copyright (c) 2008-2012 Ryan Juckett
  http://www.ryanjuckett.com/
  
  This software is provided 'as-is', without any express or implied
  warranty. In no event will the authors be held liable for any damages
  arising from the use of this software.
  
  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:
  
  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  
  3. This notice may not be removed or altered from any source
     distribution.
******************************************************************************/

/// <summary>
/// Cached set of motion parameters that can be used to efficiently update
/// multiple springs using the same time step, angular frequency and damping
/// ratio.
/// </summary>
public struct DampedSpringMotionParams
{
    // newPos = posPosCoef*oldPos + posVelCoef*oldVel
    public float m_posPosCoef, m_posVelCoef;
    // newVel = velPosCoef*oldPos + velVelCoef*oldVel
    public float m_velPosCoef, m_velVelCoef;
}

/// <summary>
/// Simple harmonic motion functions.
/// </summary>
public static class Harmonic
{

    /// <summary>
    /// This function will compute the parameters needed to simulate a damped spring over a given period of time.
    /// </summary>
    /// <param name="spring">Motion parameters result</param>
    /// <param name="deltaTime">Fixed time step to advance.</param>
    /// <param name="angularFrequency">Angular frequency of motion to control how fast the spring oscillates.</param>
    /// <param name="dampingRatio">
    /// Damping ratio of motion to control how fast the motion decays.
    /// Damping ratio > 1: over damped.
    /// Damping ratio = 1: critically damped.
    /// Damping ratio < 1: under damped.
    /// </param>
    public static void CalcDampedSpringMotionParams(out DampedSpringMotionParams spring,
        float deltaTime, float angularFrequency, float dampingRatio)
    {
        const float epsilon = 0.0001f;

        // force values into legal range
        if (dampingRatio < 0.0f) dampingRatio = 0.0f;
        if (angularFrequency < 0.0f) angularFrequency = 0.0f;

        // if there is no angular frequency, the spring will not move and we can
        // return identity
        if (angularFrequency < epsilon)
        {
            spring.m_posPosCoef = 1.0f; spring.m_posVelCoef = 0.0f;
            spring.m_velPosCoef = 0.0f; spring.m_velVelCoef = 1.0f;
            return;
        }

        if (dampingRatio > 1.0f + epsilon)
        {
            // over-damped
            float za = -angularFrequency * dampingRatio;
            float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1.0f);
            float z1 = za - zb;
            float z2 = za + zb;

            float e1 = Mathf.Exp(z1 * deltaTime);
            float e2 = Mathf.Exp(z2 * deltaTime);

            float invTwoZb = 1.0f / (2.0f * zb); // = 1 / (z2 - z1)

            float e1_Over_TwoZb = e1 * invTwoZb;
            float e2_Over_TwoZb = e2 * invTwoZb;

            float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
            float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

            spring.m_posPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
            spring.m_posVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb;

            spring.m_velPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
            spring.m_velVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
        }
        else if (dampingRatio < 1.0f - epsilon)
        {
            // under-damped
            float omegaZeta = angularFrequency * dampingRatio;
            float alpha = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio * dampingRatio);

            float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
            float cosTerm = Mathf.Cos(alpha * deltaTime);
            float sinTerm = Mathf.Sin(alpha * deltaTime);

            float invAlpha = 1.0f / alpha;

            float expSin = expTerm * sinTerm;
            float expCos = expTerm * cosTerm;
            float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

            spring.m_posPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
            spring.m_posVelCoef = expSin * invAlpha;

            spring.m_velPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
            spring.m_velVelCoef = expCos - expOmegaZetaSin_Over_Alpha;
        }
        else
        {
            // critically damped
            float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
            float timeExp = deltaTime * expTerm;
            float timeExpFreq = timeExp * angularFrequency;

            spring.m_posPosCoef = timeExpFreq + expTerm;
            spring.m_posVelCoef = timeExp;

            spring.m_velPosCoef = -angularFrequency * timeExpFreq;
            spring.m_velVelCoef = -timeExpFreq + expTerm;
        }
    }

    /// <summary>
    /// This function will update the supplied position and velocity values over
    /// according to the motion parameters.
    /// </summary>
    /// <param name="pos">position value to update</param>
    /// <param name="vel">velocity value to update</param>
    /// <param name="equilibriumPos">position to approach</param>
    /// <param name="spring">motion parameters to use</param>
    public static void UpdateDampedSpringMotion(
        ref float pos,
        ref float vel,
        in float equilibriumPos,
        in DampedSpringMotionParams spring)
    {
        float oldPos = pos - equilibriumPos; // update in equilibrium relative space
        float oldVel = vel;

        pos = oldPos * spring.m_posPosCoef + oldVel * spring.m_posVelCoef + equilibriumPos;
        vel = oldPos * spring.m_velPosCoef + oldVel * spring.m_velVelCoef;
    }

    /// <summary>
    /// This function will update the supplied position and velocity values over
    /// according to the motion parameters.
    /// </summary>
    /// <param name="pos">position value to update</param>
    /// <param name="vel">velocity value to update</param>
    /// <param name="equilibriumPos">position to approach</param>
    /// <param name="spring">motion parameters to use</param>
    public static void UpdateDampedSpringMotion(
        ref Vector2 pos,
        ref Vector2 vel,
        in Vector2 equilibriumPos,
        in DampedSpringMotionParams spring)
    {
        Vector2 oldPos = pos - equilibriumPos; // update in equilibrium relative space
        Vector2 oldVel = vel;

        pos = oldPos * spring.m_posPosCoef + oldVel * spring.m_posVelCoef + equilibriumPos;
        vel = oldPos * spring.m_velPosCoef + oldVel * spring.m_velVelCoef;
    }

    /// <summary>
    /// This function will update the supplied position and velocity values over
    /// according to the motion parameters.
    /// </summary>
    /// <param name="pos">position value to update</param>
    /// <param name="vel">velocity value to update</param>
    /// <param name="equilibriumPos">position to approach</param>
    /// <param name="spring">motion parameters to use</param>
    public static void UpdateDampedSpringMotion(
        ref Vector3 pos,
        ref Vector3 vel,
        in Vector3 equilibriumPos,
        in DampedSpringMotionParams spring)
    {
        Vector3 oldPos = pos - equilibriumPos; // update in equilibrium relative space
        Vector3 oldVel = vel;

        pos = oldPos * spring.m_posPosCoef + oldVel * spring.m_posVelCoef + equilibriumPos;
        vel = oldPos * spring.m_velPosCoef + oldVel * spring.m_velVelCoef;
    }
}
