using System.Dynamic;
using System.Net.Http;
using CS2Cheat.DTO.ClientDllDTO;
using CS2Cheat.Utils.DTO;
using Newtonsoft.Json;

namespace CS2Cheat.Utils;

public abstract class Offsets
{
    public const float WeaponRecoilScale = 2f;

    // Offsets from offsets.json
    public static int dwLocalPlayerPawn;
    public static int dwEntityList;
    public static int dwLocalPlayerController;
    public static int dwViewMatrix;
    public static int dwViewAngles;
    public static int dwPlantedC4;
    public static int dwGlobalVars;
    public static int dwGlowManager;

    // Client.dll class offsets
    public static int m_vOldOrigin;
    public static int m_vecViewOffset;
    public static int m_AimPunchAngle;
    public static int m_modelState;
    public static int m_pGameSceneNode;
    public static int m_fFlags;
    public static int m_iIDEntIndex;
    public static int m_lifeState;
    public static int m_iHealth;
    public static int m_iTeamNum;
    public static int m_bDormant;
    public static int m_iShotsFired;
    public static int m_hPawn;
    public static int m_entitySpottedState;
    public static int m_Item;
    public static int m_pClippingWeapon;
    public static int m_AttributeManager;
    public static int m_iItemDefinitionIndex;
    public static int m_bIsScoped;
    public static int m_flFlashDuration;
    public static int m_iszPlayerName;
    public static int m_nBombSite;
    public static int m_bBombDefused;
    public static int m_vecAbsVelocity;
    public static int m_flDefuseCountDown;
    public static int m_flC4Blow;
    public static int m_bBeingDefused;

    // Glow struct offsets (inside CGlowProperty)
    public static int m_bGlowEnabled;      // byte
    public static int m_glowColorOverride; // int RGBA
    public static int m_iGlowType;         // int

    // World Changer (motion blur) – placeholder, will be found via pattern
    public static IntPtr pMotionBlur;

    // Timer constant
    public const int m_nCurrentTickThisFrame = 0x34;

    // Bone dictionary (unchanged)
    public static readonly Dictionary<string, int> Bones = new()
    {
        { "head", 7 }, { "neck_0", 6 }, { "spine_1", 8 }, { "spine_2", 3 },
        { "pelvis", 1 }, { "arm_upper_L", 9 }, { "arm_lower_L", 10 }, { "hand_L", 11 },
        { "arm_upper_R", 13 }, { "arm_lower_R", 14 }, { "hand_R", 15 },
        { "leg_upper_L", 17 }, { "leg_lower_L", 18 }, { "ankle_L", 19 },
        { "leg_upper_R", 20 }, { "leg_lower_R", 21 }, { "ankle_R", 22 }
    };

    public static async Task UpdateOffsets()
    {
        using var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(10);
        try
        {
            string clientJson = await client.GetStringAsync("https://raw.githubusercontent.com/a2x/cs2-dumper/refs/heads/main/output/client_dll.json");
            string offsetsJson = await client.GetStringAsync("https://raw.githubusercontent.com/a2x/cs2-dumper/refs/heads/main/output/offsets.json");

            var clientData = JsonConvert.DeserializeObject<ClientDllDTO>(clientJson);
            var offsetsData = JsonConvert.DeserializeObject<OffsetsDTO>(offsetsJson);

            // Offsets from offsets.json (dictionary)
            var clientOffsets = offsetsData.clientdll;
            dwLocalPlayerPawn = clientOffsets["dwLocalPlayerPawn"];
            dwEntityList = clientOffsets["dwEntityList"];
            dwLocalPlayerController = clientOffsets["dwLocalPlayerController"];
            dwViewMatrix = clientOffsets["dwViewMatrix"];
            dwViewAngles = clientOffsets["dwViewAngles"];
            dwPlantedC4 = clientOffsets["dwPlantedC4"];
            dwGlobalVars = clientOffsets["dwGlobalVars"];
            dwGlowManager = clientOffsets["dwGlowManager"];

            // Client.dll class fields (from client_dll.json)
            var fields = clientData.clientdll.classes;
            m_vOldOrigin = fields.C_BasePlayerPawn.fields.m_vOldOrigin;
            m_vecViewOffset = fields.C_BaseModelEntity.fields.m_vecViewOffset;
            m_AimPunchAngle = fields.C_CSPlayerPawn.fields.m_aimPunchAngle;
            m_modelState = fields.CSkeletonInstance.fields.m_modelState;
            m_pGameSceneNode = fields.C_BaseEntity.fields.m_pGameSceneNode;
            m_iIDEntIndex = fields.C_CSPlayerPawn.fields.m_iIDEntIndex;
            m_lifeState = fields.C_BaseEntity.fields.m_lifeState;
            m_iHealth = fields.C_BaseEntity.fields.m_iHealth;
            m_iTeamNum = fields.C_BaseEntity.fields.m_iTeamNum;
            m_bDormant = fields.CGameSceneNode.fields.m_bDormant;
            m_iShotsFired = fields.C_CSPlayerPawn.fields.m_iShotsFired;
            m_hPawn = fields.CBasePlayerController.fields.m_hPawn;
            m_fFlags = fields.C_BaseEntity.fields.m_fFlags;
            m_entitySpottedState = fields.C_CSPlayerPawn.fields.m_entitySpottedState;
            m_Item = fields.C_AttributeContainer.fields.m_Item;
            m_pClippingWeapon = fields.C_CSPlayerPawnBase.fields.m_pClippingWeapon;
            m_AttributeManager = fields.C_EconEntity.fields.m_AttributeManager;
            m_iItemDefinitionIndex = fields.C_EconItemView.fields.m_iItemDefinitionIndex;
            m_bIsScoped = fields.C_CSPlayerPawnBase.fields.m_bIsScoped;
            m_flFlashDuration = fields.C_CSPlayerPawnBase.fields.m_flFlashDuration;
            m_iszPlayerName = fields.CBasePlayerController.fields.m_iszPlayerName;
            m_nBombSite = fields.C_PlantedC4.fields.m_nBombSite;
            m_bBombDefused = fields.C_PlantedC4.fields.m_bBombDefused;
            m_vecAbsVelocity = fields.C_BaseEntity.fields.m_vecAbsVelocity;
            m_flDefuseCountDown = fields.C_PlantedC4.fields.m_flDefuseCountDown;
            m_flC4Blow = fields.C_PlantedC4.fields.m_flC4Blow;
            m_bBeingDefused = fields.C_PlantedC4.fields.m_bBeingDefused;

            // Glow struct offsets (typical values – update from dumper if available)
            m_bGlowEnabled = 0x28;
            m_glowColorOverride = 0x2C;
            m_iGlowType = 0x30;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[Offsets] Updated successfully. dwLocalPlayerPawn = 0x{dwLocalPlayerPawn:X}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Offsets] Failed: {ex.Message}");
            throw;
        }
    }
}