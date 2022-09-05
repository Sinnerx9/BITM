
namespace BITM.BlazeSDK
{

    public class Enums
    {
        public enum client_type
        {
            //havana_server,
            //havana_client,
            ittakestwo,
            unknown
        };
        public enum Component : ushort
        {
            FIRE2CONNECTION = 0x0,
            AUTHENTICATION = 0x1,
            GAMEMANAGER = 0x4,
            REDIRECTOR = 0x5,
            SQUAD = 0x6,
            STATS = 0x7,
            UTIL = 0x9,
            CENSUSDATA = 0xA,
            CLUBS = 0xB,
            MESSAGING = 0xF,
            ASSOCIATIONLIST = 0x19,
            ACOUNT = 0x23,
            GAMEREPORTING = 0x1C,
            RSP = 0x801,
            PACKS = 0x802,
            INVENTORY = 0x803,
            QUARTER = 0x804,
            COMPETITIVE = 0x806,
            USERSESSIONS = 0x7802
        };
        public enum MessageType : ushort
        {
            REQUEST = 0x0000,
            MESSAGE = 0x1000,
            REPLY = 0x2000,
            NOTIFICATION_TYPE_0 = 0x4000,
            NOTIFICATION_TYPE_1 = 0x4001,
            ERROR_REPLY = 0x6000,
            LARGE_RESPOSNE = 0x1010,
            PING_REQUEST = 0x8000,
            PING_RESPONSE = 0xA000
        };
        public enum AssociationListType : ushort
        {
            FriendsList = 1,
            RecentPlayerList = 2,
            PersistentMuteList = 3,
            CommunicationBlockList = 4
        };
        public enum GameState : ushort
        {
            NEW_STATE = 0x0,
            INITIALIZING = 0x1,
            VIRTUAL = 0x2,
            PRE_GAME = 0x82,
            IN_GAME = 0x83,
            POST_GAME = 0x4,
            MIGRATING = 0x5,
            DESTRUCTING = 0x6,
            RESETABLE = 0x7,
            REPLAY_SETUP = 0x8
        };
        public enum ErrorCodes : long
        {
            //USERSETTINGS
            USERSETTING_NOT_FOUND = 0xC80009,

            //NEEDS SDK_ERR codes
            //NEEDS GAMEMANAGER_ERR codes
            //NEEDS GAMEHISTORY_ERR codes
            //NEEDS GAMEREPORTING_ERR codes

            //GAMEMANAGER_ERR
            //FOR USE WITH 0x0007 (GameManager Component and MessageType.ERROR)!!!!!!!!!!!!!!!
            GAMEMANAGER_ERR_INVALID_GAME_ID = 0x2004,

            //FOR USE WITH 0x7802 (UserSessions Component and MessageType.ERROR)!!!!!!!!!!!!!!!
            USER_ERR_KEY_NOT_FOUND = 0x0006,
            USER_ERR_USER_NOT_FOUND = 0x0001,
            USER_ERR_SESSION_NOT_FOUND = 0x0002,
            USER_ERR_DUPLICATE_SESSSION = 0x0003,
            USER_ERR_NO_EXTENDED_DATA = 0x0004,
            USER_ERR_MAX_DATA_REACHED = 0x0005,
            USER_ERR_INVALID_SESSION_INSTANCE = 0x0007,
            USER_ERR_INVALID_PARAM = 0x0008,
            USER_ERR_MINIMUM_CHARACTERS = 0x0009,

            ACCESS_GROUP_ERR_INVALID_GROUP = 0x000A,
            ACCESS_GROUP_ERR_DEFAULT_GROUP = 0x000B,
            ACCESS_GROUP_ERR_NOT_CURRENT_GROUP = 0x000C,
            ACCESS_GROUP_ERR_CURRENT_GROUP = 0x000D,
            ACCESS_GROUP_ERR_NO_GROUP_FOUND = 0x000E,

            GEOIP_INCOMPLETE_PARAMETERS = 0x000F,
            GEOIP_UNABLE_TO_RESOLVE = 0x0010,

            ENTITY_TYPE_NOT_FOUND = 0x0011,
            ENTITY_NOT_FOUND = 0x0012,
            NOT_SUPPORTED = 0x0013,

            USER_ERR_EXISTS = 0x0014,
            USER_ERR_RESUMABLE_SESSION_CONNECTION_INVALID = 0x0015,
            USER_ERR_RESUMABLE_SESSION_NOT_FOUND = 0x0016,

            GEOIP_ERR_USER_OUTPUT = 0x0017,
            ////////////////////////

            //FOR USE WITH 0x0009 (Util Component and MessageType.ERROR)!!!!!!!!!!!!!!!
            UTIL_CONFIG_NOT_FOUND = 0x0064,

            UTIL_PSS_NO_SERVERS_AVAILABLE = 0x0091,

            UTIL_TELEMETRY_NO_SERVERS_AVAILABLE = 0x0096,
            UTIL_TELEMETRY_OUT_OF_MEMORY = 0x0097,
            UTIL_TELEMETRY_KEY_TOO_LONG = 0x0098,
            UTIL_TELEMETRY_INVALID_MAC_ADDRESS = 0x0099,

            UTIL_TICKER_NO_SERVERS_AVAILABLE = 0x009B,
            UTIL_TICKER_KEY_TOO_LONG = 0x009C,

            UTIL_USS_USER_NO_EXTENDED_DATA = 0x00FA,
            UTIL_USS_RECORD_NOT_FOUND = 0x00C8,
            UTIL_USS_TOO_MANY_KEYS = 0x00C9,
            UTIL_USS_DB_ERROR = 0x00CA,
            ////////////////////////

            SYSTEM = 0x40010000,
            COMPONENT_NOT_FOUND = 0x40020000,
            COMMAND_NOT_FOUND = 0x40030000,
            AUTHENTICATION_REQUIRED = 0x40040000,
            TIMEOUT = 0x40050000,
            DISCONNECTED = 0x40060000,
            DUPLICATE_LOGIN = 0x40070000,
            AUTHORIZATION_REQUIRED = 0x40080000,
            CANCELED = 0x40090000,
            CUSTOM_REQUEST_HOOK_FAILED = 0x400A0000,
            CUSTOM_RESPONSE_HOOK_FAILED = 0x400B0000,
            STRING_TOO_LONG = 0x400C0000,
            INVALID_ENUM_VALUE = 0x400D0000,

            DB_SYSTEM = 0x40650000,
            DB_NOT_CONNECTED = 0x40660000,
            DB_NOT_SUPPORTED = 0x40670000,
            DB_NO_CONNECTION_AVAILABLE = 0x40680000,
            DB_DUP_ENTRY = 0x40690000,
            DB_NO_SUCH_TABLE = 0x406A0000,
            DB_DISCONNECTED = 0x406B0000,
            DB_TIMEOUT = 0x406C0000,
            DB_INIT_FAILIED = 0x406D0000,
            DB_TRANSACTION_NOT_COMPLETE = 0x406E0000,
            DB_LOCK_DEADLOCK = 0x406F0000,
            DB_DROP_PARTITION_NON_EXISTENT = 0x40700000,
            DB_SAME_NAME_PARITION = 0x40710000,
            SERVER_NOT_FOUND = 0x100050000,
            SERVER_BUSY = 0x40720000,
            GUEST_SESSION_NOT_ALLOWED = 0x40730000
        }
        public enum c_GameManager
        {
            CreateGame = 0x01,
            DestroyGame = 0x02,
            AdvanceGameState = 0x03,
            JoinGame = 0x09,
            RemovePlayer = 0x0B,
            FinalizeGameCreation = 0x0F,
            LeaveGameByGroup = 0x16,
            MeshEndpointsConnected = 0x41,
            MeshEndpointsDisConnected = 0x42,
            MeshEndpointsConnectionLost = 0x43,
            ReportTelemetry = 0xAB,
            NotifyGameSetup = 0x14,
            NotifyGameStateChange = 0x64,
            ejectHost = 0x28
        }
        public enum c_Authentication
        {
            LogOut = 0x46,
            CreateAccount = 0x0A,
        }
        public enum c_UserSessions
        {
            
            UpdateHardwareFlags = 0x08,
            UpdateNetworkInfo = 0x14,
            UserAdded= 0x02,
            UserSessionExtendedDataUpdate = 0x01
        }
        public enum c_Util
        {
            SuspendUserPing= 0x1B,
            Ping = 0x02,
            PreAuth = 0x07,
            PostAuth = 0x08,
            GetTelemetryServer = 0x05,
            setClientMetrics = 0x16
            

        }
    }
}
