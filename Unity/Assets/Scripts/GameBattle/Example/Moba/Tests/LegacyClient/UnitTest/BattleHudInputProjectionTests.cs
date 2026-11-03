using AbilityKit.Game.Battle.View;
using NUnit.Framework;
using UnityEngine;

namespace AbilityKit.Game.Test.UnitTest
{
    public sealed class BattleHudInputProjectionTests
    {
        [Test]
        public void ToCameraPlane_WithoutCameraTransform_ReturnsInput()
        {
            var input = new Vector2(0.25f, -0.75f);

            var projected = BattleHudInputProjection.ToCameraPlane(input, null);

            Assert.AreEqual(input, projected);
        }

        [Test]
        public void ToCameraPlane_UsesCameraYawForMoveAndAimDirection()
        {
            var camera = new GameObject("BattleHudInputProjectionTests.Camera").transform;
            try
            {
                camera.rotation = Quaternion.Euler(0f, 90f, 0f);

                var forward = BattleHudInputProjection.ToCameraPlane(Vector2.up, camera);
                var right = BattleHudInputProjection.ToCameraPlane(Vector2.right, camera);

                AssertVector2(new Vector2(1f, 0f), forward);
                AssertVector2(new Vector2(0f, -1f), right);
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        [Test]
        public void ToCameraPlane_IgnoresCameraPitch()
        {
            var camera = new GameObject("BattleHudInputProjectionTests.PitchedCamera").transform;
            try
            {
                camera.rotation = Quaternion.Euler(45f, 0f, 0f);

                var projected = BattleHudInputProjection.ToCameraPlane(Vector2.up, camera);

                AssertVector2(new Vector2(0f, 1f), projected);
            }
            finally
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        private static void AssertVector2(Vector2 expected, Vector2 actual)
        {
            Assert.AreEqual(expected.x, actual.x, 0.0001f);
            Assert.AreEqual(expected.y, actual.y, 0.0001f);
        }
    }
}
