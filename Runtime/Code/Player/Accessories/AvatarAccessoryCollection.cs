using UnityEngine;

namespace Code.Player.Accessories {
	[CreateAssetMenu(fileName = "AvatarCollection", menuName = "Airship/Accessories/Avatar Accessory Collection", order = 0)]
	public class AvatarAccessoryCollection : ScriptableObject{
		public AccessoryComponent[] accessories;
		public AccessoryFace[] faces;
		public Color[] skinColors;
	}
}
