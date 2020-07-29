using UnityEngine;

namespace AwesomeTechnologies.Utility
{
    public static class AnimationCurveExtention
    {
        public static float[] GenerateCurveArray(this AnimationCurve self, int sampleCount)
        {
            float[] returnArray = new float[sampleCount];
            for (int j = 0; j <= sampleCount-1; j++)
                returnArray[j] = self.Evaluate(j / (float)sampleCount);

            return returnArray;
        }
    }
}
