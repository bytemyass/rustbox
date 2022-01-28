using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RustDMA
{
    public static class Offsets
    {
        public enum PlayerFlags // TypeDefIndex: 8879
        {
            Unused1 = 1,
            Unused2 = 2,
            IsAdmin = 4,
            ReceivingSnapshot = 8,
            Sleeping = 16,
            Spectating = 32,
            Wounded = 64,
            IsDeveloper = 128,
            Connected = 256,
            ThirdPersonViewmode = 1024,
            EyesViewmode = 2048,
            ChatMute = 4096,
            NoSpr = 8192,
            Aiming = 16384,
            DisplaySash = 32768,
            Relaxed = 65536,
            SafeZone = 131072,
            ServerFall = 262144,
            Incapacitated = 524288,
            Workbench1 = 1048576,
            Workbench2 = 2097152,
            Workbench3 = 4194304
        }


        public static uint gom = 0x17C1F18;
        public static uint baseNetworkable = 0x3115CB0;
        public static uint occlusionculling = 0x31161F0; //OcclusionCulling_Typeinfo  > scripts.json

        public static uint playerFlags = 0x680; //BasePlayer
        public static uint playerEyes = 0x688; //BasePlayer

        public static uint health = 0x224; //BaseCombatEntity _health
        public static uint playerInventory = 0x690; //BasePlayer
        public static uint clActiveItem = 0x5D0; //BasePlayer
        public static uint recoilProperties = 0x2D8; //BaseProjectile

        public static uint playerModel = 0x4C0; //BasePlayer
        public static uint skinSetWomen = 0x198; //Playermodel
        public static uint skinSetMale = 0x190; //Playermodel
        public static uint needsClothingRebuild = 0x520; //Playermodel


        public static uint clothingBlocksAiming = 0x750; // BasePlayer : BaseCombatEntity
        public static uint clothingMoveSpeedReduction = 0x754; // BasePlayer : BaseCombatEntity
        public static uint equippingBlocked = 0x760; // BasePlayer : BaseCombatEntity
        public static uint playerBaseMovement = 0x4e8; // BasePlayer

        public static uint groundAngle = 0xc4;     // PlayerWalkMovement : BaseMovement
        public static uint groundAngleNew = 0xc8;  // PlayerWalkMovement : BaseMovement
        public static uint jumpTime = 0xd0;        // PlayerWalkMovement : BaseMovement
        public static uint landTime = 0xd4;        // PlayerWalkMovement : BaseMovement
        public static uint groundTime = 0xcc;      // PlayerWalkMovement : BaseMovement

        public static uint cameraFov = 0x18;       // BasePlayer : BaseCombatEntity
        public static uint CameraManager = 0x18;   // BasePlayer : BaseCombatEntity

        public static uint debugShow = 0x94; //
        public static uint debugSettings = 0x18;  //
    }
}
