using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AwesomeTechnologies.Utility
{
	public enum ObstacleMaskQuality
	{
		Low1024 = 0,
		Normal2048 = 1,
		High4096 = 2,
		Ultra8192 = 3
	}
	
	public class ObstacleMaskCreator : MonoBehaviour
	{
		public ObstacleMaskQuality ObstacleMaskQuality = ObstacleMaskQuality.High4096;
		public LayerMask LayerMask = 0;
		public float MinimumDistance = 0;
		public Rect AreaRect;

		public int GetObstacleMaskQualityPixelResolution(ObstacleMaskQuality obstacleMaskQuality)
		{
			switch (ObstacleMaskQuality)
			{
				case ObstacleMaskQuality.Low1024:
					return 1024;
				case ObstacleMaskQuality.Normal2048:
					return 2048;
				case ObstacleMaskQuality.High4096:
					return 4096;
				case ObstacleMaskQuality.Ultra8192:
					return 8192;
				default:
					return 1024;
			}
		}
	}
}
