using ElRaccoone.Tweens.Core;
using UnityEngine;

namespace ElRaccoone.Tweens {
  public static partial class NativeTween {
    public static Tween<float> PositionZ (this Component self, float to, float duration) =>
      Tween<float>.Add<PositionZDriver> (self).Finalize (to, duration);

    public static Tween<float> PositionZ (this GameObject self, float to, float duration) =>
      Tween<float>.Add<PositionZDriver> (self).Finalize (to, duration);

    /// <summary>
    /// The driver is responsible for updating the tween's state.
    /// </summary>
    private class PositionZDriver : TweenComponent<float, Transform> {
      private Vector3 position;

      /// <summary>
      /// Overriden method which is called when the tween starts and should
      /// return the tween's initial value.
      /// </summary>
      public override float OnGetFrom () {
        return this.component.position.z;
      }

      /// <summary>
      /// Overriden method which is called every tween update and should be used
      /// to update the tween's value.
      /// </summary>
      /// <param name="easedTime">The current eased time of the tween's step.</param>
      public override void OnUpdate (float easedTime) {
        this.position = this.component.position;
        this.valueCurrent = this.InterpolateValue (this.valueFrom, this.valueTo, easedTime);
        this.position.z = this.valueCurrent;
        this.component.position = this.position;
      }
    }
  }
}