using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace rustbox.Features
{
    public static class VectorExtensions
    {
        public static System.Numerics.Vector3 RotateY(this System.Numerics.Vector3 vector, double yaw)
        {
            var s = Math.Sin(yaw);
            var c = Math.Cos(yaw);

            return new System.Numerics.Vector3(
                Convert.ToSingle(vector.X * c - vector.Z * s),
                vector.Y,
                Convert.ToSingle(vector.X * s + vector.Z * c)
            );
        }
    }

    public static class UpdateLoop
    {
        public static bool noRecoil = false;
        public static bool adminFlag = false;
        public static bool keepRecoil = false;

        public static bool viewOffset = false;
        public static bool spiderman = false;
        public static bool canJump = false;

        public static bool setFov = false;
        public static float fov = 90;
        public static bool cullingESP = false;

        public static FormMain formMain;

        private static ulong localPlayer = 0;
        private static ulong GameObjectManager = 0;
        private static ulong TOD_Sky = 0;

        private static bool restoredRecoil = false;
        private static ulong lastWeaponPtr = 0;
        private static uint lastItemId;
      

        public static bool rewrite = false;

        public struct Weapon
        {
            public int oMaxYaw;
            public int oMaxPitch;
            public float multiplierYaw;
            public float multiplierPitch;

            public Weapon(int oMaxYaw, int oMaxPitch, float multiplierYaw, float multiplierPitch)
            {
                this.oMaxPitch = oMaxPitch;
                this.oMaxYaw = oMaxYaw;
                this.multiplierYaw = multiplierYaw;
                this.multiplierPitch = multiplierPitch;
            }
        }

        public static Dictionary<uint, Weapon> weaponDict = new Dictionary<uint, Weapon>();
        public static volatile Dictionary<ulong, GameObject> cachedEntities = new Dictionary<ulong, GameObject>();

        public static void RunEntityLoop()
        {
            GameObjectManager = DMAController.ReadMemory<ulong>(DMAController.unityplayer.vaBase + Offsets.gom);

            ulong baseNetworkable = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.baseNetworkable);
            baseNetworkable = DMAController.ReadMemory<ulong>(baseNetworkable + 0xb8);
            baseNetworkable = DMAController.ReadMemory<ulong>(baseNetworkable);

            var ffirstptr = DMAController.ReadMemory<ulong>(baseNetworkable + 0x10);
            var secondPTR = DMAController.ReadMemory<ulong>(ffirstptr + 0x28);
            var ThirdPTR = DMAController.ReadMemory<ulong>(secondPTR + 0x18);

            TOD_Sky = FindSkyDome();

            int prevSelectedChams = 0;

            while (true)
            {
                if (formMain.checkBoxInteractiveDebugCamera.Checked)
                    continue;

                List<ulong> control = new List<ulong>();
                var objectPtr = DMAController.ReadMemory<ulong>(ThirdPTR + 0x20 + (0 * 0x8));
                var localPlayerGameObject = DMAController.ReadMemory<ulong>(objectPtr + 0x10);
                localPlayer = DMAController.ReadMemory<ulong>(localPlayerGameObject + 0x30);

                var listLenght = DMAController.ReadMemory<int>(secondPTR + 0x10);

                byte[] buffer = vmm.MemRead(DMAController.pid, ThirdPTR + 0x20, (uint)(sizeof(ulong) * listLenght));
                Span<ulong> ptrs = MemoryMarshal.Cast<byte, ulong>(buffer);

                bool needsRewrite = formMain.needRewrite;

                for (int i = 1; i < ptrs.Length; i++)
                {
                    if (ptrs[i] != 0)
                    {
                        control.Add(ptrs[i]);

                        var baseObject = DMAController.ReadMemory<ulong>(ptrs[i] + 0x10);
                        var ent = DMAController.ReadMemory<ulong>(baseObject + 0x28);
                        var entClass = DMAController.ReadMemory<ulong>(ent);
                        var classNamePtr = DMAController.ReadMemory<ulong>(entClass + 0x10);
                        var className = DMAController.ReadClassName(classNamePtr);

                        GameObject cachedGom;
                        if (cachedEntities.TryGetValue(ptrs[i], out cachedGom))
                        {
                            if (className != "BasePlayer")
                                continue;
                        }
                        else
                        {
                            cachedEntities.Add(ptrs[i], new GameObject());
                        }

                        if (className == "BasePlayer" && formMain.checkBoxChams.Checked)
                        {
                            ulong shit = DMAController.ReadMemory<ulong>(baseObject + 0x30);
                            ulong objectClasses = DMAController.ReadMemory<ulong>(shit + 0x30);
                            ulong Entity = DMAController.ReadMemory<ulong>(objectClasses + 0x18);
                            ulong baseEntity = DMAController.ReadMemory<ulong>(Entity + 0x28);

                            if (baseEntity != 0 && TOD_Sky != 0)
                            {
                                ulong components = DMAController.ReadMemory<ulong>(TOD_Sky + 0xA8);
                                ulong scattering = DMAController.ReadMemory<ulong>(components + 0x1A0);
                                ulong material = DMAController.ReadMemory<ulong>(scattering + 0x78);

                                //set player skin to cham material
                                ulong playerModel = DMAController.ReadMemory<ulong>(baseEntity + Offsets.playerModel); //playerModel in Baseplayer
                                ulong skinSet = DMAController.ReadMemory<ulong>(playerModel + Offsets.skinSetWomen);
                                ulong skinSetMale = DMAController.ReadMemory<ulong>(playerModel + Offsets.skinSetMale);

                                SetMaterial(skinSetMale, material);
                                SetMaterial(skinSet, material);

                                if (needsRewrite)
                                    DMAController.WriteMemory<bool>(baseEntity + Offsets.needsClothingRebuild, true); //needs clothing rebuild
                            }
                        }
                    }
                }

                if (needsRewrite)
                    formMain.needRewrite = false;
            }
        }

        public static void SetMaterial(ulong skinset, ulong material)
        {
            if (skinset != 0)
            {
                ulong skins = DMAController.ReadMemory<ulong>(skinset + 0x18);
                int size = DMAController.ReadMemory<int>(skins + 0x18);

                if (size < 20)
                {
                    for (int e = 0; e < size; e++)
                    {
                        ulong currentSkinSet = DMAController.ReadMemory<ulong>(skins + 0x20 + (ulong)(e * 0x8));

                        if (currentSkinSet != 0)
                        {

                            if (material != 0)
                            {
                                if (formMain.selectedChams == 0)
                                {
                                    DMAController.WriteMemory<ulong>(currentSkinSet + 0x68, material);
                                    DMAController.WriteMemory<ulong>(currentSkinSet + 0x70, material);
                                    DMAController.WriteMemory<ulong>(currentSkinSet + 0x78, material);
                                }
                                else
                                {
                                    DMAController.WriteMemory<ulong>(currentSkinSet + 0x68, 0);
                                    DMAController.WriteMemory<ulong>(currentSkinSet + 0x70, 0);
                                    DMAController.WriteMemory<ulong>(currentSkinSet + 0x78, 0);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void DestroyOldEntities(List<ulong> control)
        {
            try
            {
                ulong[] controlLocalCopy = new ulong[control.Count];
                control.CopyTo(controlLocalCopy);

                for (int i = 0; i < cachedEntities.Count; i++)
                {
                    if (!controlLocalCopy.Contains(cachedEntities.ElementAt(i).Key))
                    {
                        cachedEntities.Remove(cachedEntities.ElementAt(i).Key);
                    }
                }
            }
            catch (Exception ex) { }
        }

        [DllImport("User32.dll")]
        private static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey); // Keys enumeration

        private static double previousYaw = 0;
        private static float camSpeed = 0.00015f;
        private static float camSpeedMultiplier = 5;
        private static bool camFast = false;
        private static float camDrag = 0.99f;
        private static bool camFlyToLook = true;
        private static System.Numerics.Vector3 camVelocity = new System.Numerics.Vector3();

        private static System.Numerics.Vector3 targetmovement = new System.Numerics.Vector3();

        private static System.Numerics.Vector3 forward = new System.Numerics.Vector3(0, 0, 1);
        private static System.Numerics.Vector3 right = new System.Numerics.Vector3(1, 0, 0);
        private static System.Numerics.Vector3 up = new System.Numerics.Vector3(0, 1, 0);

        private static System.Numerics.Vector3 GetForwardDirection(System.Numerics.Quaternion quaternion)
        {
            return new System.Numerics.Vector3(
                2 * (quaternion.X * quaternion.Z + quaternion.W * quaternion.Y),
                2 * (quaternion.Y * quaternion.Z - quaternion.W * quaternion.X),
                1 - 2 * (quaternion.X * quaternion.X - quaternion.Y * quaternion.Y)
                );
        }

        private static double GetYawRad(System.Numerics.Quaternion q)
        {
            return Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * q.Y * q.Y - 2 * q.Z * q.Z);
        }

        public static void RunFeatures()
        {
            Stopwatch stopwatch = new Stopwatch();

            while (true)
            {
                var deltaTime = Convert.ToSingle(stopwatch.Elapsed.TotalMilliseconds);
                stopwatch.Restart();

                var objectClasses = DMAController.ReadMemory<ulong>(localPlayer + 0x30);
                var entity = DMAController.ReadMemory<ulong>(objectClasses + 0x18);
                var baseEntity = DMAController.ReadMemory<ulong>(entity + 0x28);
                var baseMovement = DMAController.ReadMemory<ulong>(baseEntity + Offsets.playerBaseMovement);
                if (formMain.checkBoxInteractiveDebugCamera.Checked && localPlayer != 0)
                {
                    // Read Memory
                    ulong playerEyes = DMAController.ReadMemory<ulong>(baseEntity + Offsets.playerEyes); //playerEyes in Baseplayer
                    System.Numerics.Quaternion currentRotation = DMAController.ReadMemory<System.Numerics.Quaternion>(playerEyes + 0x44);

                    // Calculate how much the camera turned around the Y axis
                    var currentYaw = GetYawRad(currentRotation);
                    var deltaRotation = currentYaw - previousYaw; // We need to know how much the camera moved since the last time
                    previousYaw = currentYaw; // Save the yaw
                    // If the camera has turned
                    if (deltaRotation != 0)
                        // Fix the position
                        targetmovement = targetmovement.RotateY(deltaRotation);

                    // Check for reset
                    if (GetAsyncKeyState(System.Windows.Forms.Keys.R) != 0)
                    {
                        camVelocity = new System.Numerics.Vector3();
                        targetmovement = new System.Numerics.Vector3();
                    }

                    // Switch camera mode hotkey
                    if (GetAsyncKeyState(System.Windows.Forms.Keys.Q) != 0)
                        camFlyToLook = !camFlyToLook;

                    //Add movement vector
                    if (GetAsyncKeyState(System.Windows.Forms.Keys.LShiftKey) != 0)
                        camFast = true;
                    else
                        camFast = false;

                    int moveCam = 0;
                    if (GetAsyncKeyState(System.Windows.Forms.Keys.W) != 0)
                    {
                        camVelocity += forward;
                        moveCam = 1;
                    }

                    if (GetAsyncKeyState(System.Windows.Forms.Keys.S) != 0)
                    {
                        camVelocity -= forward;
                        moveCam = -1;
                    }

                    if (GetAsyncKeyState(System.Windows.Forms.Keys.A) != 0)
                        camVelocity -= right;
                    if (GetAsyncKeyState(System.Windows.Forms.Keys.D) != 0)
                        camVelocity += right;

                    // Up/Down movement
                    if (camFlyToLook)
                    {
                        // Add up/down component from view direction
                        camVelocity.Y += GetForwardDirection(currentRotation).Y * moveCam;
                    }
                    else
                    {
                        if (GetAsyncKeyState(System.Windows.Forms.Keys.LControlKey) != 0)
                            camVelocity -= up;
                        if (GetAsyncKeyState(System.Windows.Forms.Keys.Space) != 0)
                            camVelocity += up;
                    }


                    // Apply velocity to pos
                    if (camFast)
                        targetmovement += camVelocity * deltaTime * camSpeed * camSpeedMultiplier;
                    else
                        targetmovement += camVelocity * deltaTime * camSpeed;

                    // Apply drag to velocity
                    camVelocity *= camDrag;

                    DMAController.WriteMemory<System.Numerics.Vector3>(playerEyes + 0x38, targetmovement);

                    continue;
                }

                //j7ware additions//

                if (cullingESP && localPlayer != 0) /// this one makes it constantly write, we dont know if this is necessary yet so use the buttons
                {
                    ulong occlusionCulling = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + Offsets.occlusionculling);
                    ulong otherCulling = DMAController.ReadMemory<ulong>(occlusionCulling + 0xB8);




                    DMAController.WriteMemory<bool>(otherCulling + Offsets.debugShow, true);

                }

                if (setFov && localPlayer != 0)
                {
                    ulong camManager = DMAController.ReadMemory<ulong>(DMAController.gameAssembly.vaBase + 0x31138F0);
                    ulong camMan = DMAController.ReadMemory<ulong>(camManager + 0xB8);

                    DMAController.WriteMemory<float>(camMan + Offsets.cameraFov, fov);
                }

                if (canJump && localPlayer != 0)
                {

                    DMAController.WriteMemory<float>(baseMovement + Offsets.jumpTime, 0.0f);
                    DMAController.WriteMemory<float>(baseMovement + Offsets.landTime, 0.0f);
                    DMAController.WriteMemory<float>(baseMovement + Offsets.groundTime, 2500.0f);

                }

                if (spiderman && localPlayer != 0)
                {
                    DMAController.WriteMemory<float>(baseMovement + Offsets.groundAngle, 0.0f);
                    DMAController.WriteMemory<float>(baseMovement + Offsets.groundAngleNew, 0.0f);
                }
                ///////
                ////
                //

                if (adminFlag && localPlayer != 0)
                {
                    float health = DMAController.ReadMemory<float>(baseEntity + Offsets.health);
                    int playerFlags = DMAController.ReadMemory<int>(baseEntity + Offsets.playerFlags);

                    int sleeping = (playerFlags & (int)Offsets.PlayerFlags.Sleeping);
                    int wounded = (playerFlags & (int)Offsets.PlayerFlags.Wounded);

                    if (sleeping == 0 && wounded == 0 && health > 0)
                    {
                        playerFlags |= (int)Offsets.PlayerFlags.IsAdmin;
                        DMAController.WriteMemory<int>(baseEntity + Offsets.playerFlags, playerFlags);

                    }
                }

                if (noRecoil)
                {
                    var inventory = DMAController.ReadMemory<ulong>(baseEntity + Offsets.playerInventory);
                    var belt = DMAController.ReadMemory<ulong>(inventory + 0x28);

                    for (int i = 0; i < 6; i++)
                    {
                        var contentsItemList = DMAController.ReadMemory<ulong>(belt + 0x38);
                        var activeItem = DMAController.ReadMemory<int>(baseEntity + Offsets.clActiveItem);
                        var Items = DMAController.ReadMemory<ulong>(contentsItemList + 0x10);
                        var item = DMAController.ReadMemory<ulong>((ulong)((IntPtr)Items + 0x20 + (i * 0x8)));
                        var currID = DMAController.ReadMemory<int>(item + 0x28);

                        if (item != 0 && currID == activeItem)
                        {
                            ulong ItemInfo = DMAController.ReadMemory<ulong>(item + 0x20);
                            var ItemId = DMAController.ReadMemory<uint>(ItemInfo + 0x18);

                            if (ItemId != lastItemId)
                                RestoreRecoil();

                            Weapon weapon;
                            if (weaponDict.TryGetValue(ItemId, out weapon))
                            {
                                ulong baseprojectile = DMAController.ReadMemory<ulong>(item + 0x98);

                                if (ItemId != lastItemId || rewrite)
                                {
                                    ulong recoilProperties = DMAController.ReadMemory<ulong>(baseprojectile + Offsets.recoilProperties);

                                    if (baseprojectile != 0 && recoilProperties != 0)
                                    {
                                        DMAController.WriteMemory<float>(recoilProperties + 0x1C, weapon.oMaxYaw * weapon.multiplierYaw); //writing maxYaw
                                        DMAController.WriteMemory<float>(recoilProperties + 0x24, weapon.oMaxPitch * weapon.multiplierPitch); //writing maxPitch
                                    }
                                    rewrite = false;
                                }

                                lastWeaponPtr = baseprojectile;
                                restoredRecoil = false;
                            }

                            lastItemId = ItemId;
                        }
                    }
                }
                else
                {
                    if (!restoredRecoil)
                    {
                        RestoreRecoil();
                        restoredRecoil = true;

                        restoredRecoil = false;
                        lastWeaponPtr = 0;
                        lastItemId = 0;
                    }
                }
            }
        }

        private static void RestoreRecoil()
        {
            if (!keepRecoil)
            {
                Weapon weapon;
                if (weaponDict.TryGetValue(lastItemId, out weapon))
                {
                    ulong recoilProperties = DMAController.ReadMemory<ulong>(lastWeaponPtr + Offsets.recoilProperties);

                    if (recoilProperties != 0)
                    {
                        DMAController.WriteMemory<float>(recoilProperties + 0x1C, weapon.oMaxYaw); //writing maxYaw
                        DMAController.WriteMemory<float>(recoilProperties + 0x24, weapon.oMaxPitch); //writing maxPitch
                    }
                }
            }
        }

        public static ulong FindSkyDome()
        {
            ulong firstObject = DMAController.ReadMemory<ulong>(GameObjectManager + 0x8);
            ulong nextObject = firstObject;
            do
            {
                ulong gameObject = DMAController.ReadMemory<ulong>(nextObject + 0x10);
                ulong namePtr = DMAController.ReadMemory<ulong>(gameObject + 0x60);
                string gameobjectName = DMAController.ReadString(namePtr);
                nextObject = DMAController.ReadMemory<ulong>(nextObject + 0x8);

                if (gameobjectName.Contains("Sky"))
                {
                    ulong objectClasses = DMAController.ReadMemory<ulong>(gameObject + 0x30);
                    ulong Entity = DMAController.ReadMemory<ulong>(objectClasses + 0x18);
                    ulong baseEntity = DMAController.ReadMemory<ulong>(Entity + 0x28);
                    return baseEntity;
                }
            }
            while (nextObject != firstObject && firstObject != 0); //loops through all gameObjects
            return 0;
        }
    }
}
