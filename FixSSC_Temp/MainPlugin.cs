using Newtonsoft.Json;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace FixSSC_Temp
{
    [ApiVersion(2, 1)]
    public class MainPlugin : TerrariaPlugin
    {
        public override Version Version => new Version(1, 0, 0, 2);
        public override string Name => "FixSSC_Temp";
        public override string Description => "临时修复ssc";
        public override string Author => "Leader";
        public override bool Enabled => TShockAPI.TShock.ServerSideCharacterConfig.Settings.Enabled;

        /// <summary>
        /// 当前玩家装备数据
        /// </summary>
        Dictionary<string, Player> Now { get; set; } = new Dictionary<string, Player>();
        /// <summary>
        /// 玩家进服前装备数据
        /// </summary>
        Dictionary<string, Player> Source { get; set; } = new Dictionary<string, Player>();

        public MainPlugin(Main game) : base(game)
        {

        }

        public override void Initialize()
        {
            GetDataHandlers.PlayerSlot.Register(OnPlayerSlot);
            TShockAPI.Hooks.PlayerHooks.PlayerLogout += PlayerHooks_PlayerLogout;
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
            ServerApi.Hooks.NetGetData.Register(this, OnNetGetData);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnGamePostInit);
            ServerApi.Hooks.WorldSave.Register(this, OnWorldSave);
            ServerApi.Hooks.ServerLeave.Register(this, OnPlayerLeave);
        }

        private void OnPlayerLeave(LeaveEventArgs args)
        {
            var player = TShock.Players[args.Who];
            if (Now.ContainsKey(player.Name))
            {
                Now[player.Name].Update(new List<string>() { "Name" }, "Dye0", "Dye1", "Dye2", "Armor0", "Armor1", "Armor2", "CurrentLoadoutIndex");
                Now.Remove(player.Name);
            }
            if (Source.ContainsKey(player.Name))
            {
                //切换到装备栏1
                NetMessage.TrySendData(147, -1, -1, null, player.Index, 0);
                var now = Source[player.Name];
                for (var i = 0; i < 20; i++)
                {
                    SetPlayerInvSlot(player, i + Terraria.ID.PlayerItemSlotID.Armor0 + i, now.Armor0[i]);
                    SetPlayerInvSlot(player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0 + i, now.Armor1[i]);
                    SetPlayerInvSlot(player, i + Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0 + i, now.Armor2[i]);
                }
                for (var i = 0; i < 10; i++)
                {
                    SetPlayerInvSlot(player, i + Terraria.ID.PlayerItemSlotID.Dye0 + i, now.Dye0[i]);
                    SetPlayerInvSlot(player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 + i, now.Dye1[i]);
                    SetPlayerInvSlot(player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 + i, now.Dye2[i]);
                }
                Source.Remove(player.Name);
            }
        }

        private void OnWorldSave(WorldSaveEventArgs args)
        {
            foreach (var player in Now.Keys)
            {
                if (Now.ContainsKey(player))
                {
                    Now[player].Update(new List<string>() { "Name" }, "Dye0", "Dye1", "Dye2", "Armor0", "Armor1", "Armor2", "CurrentLoadoutIndex");
                }
            }
        }

        private void OnGamePostInit(EventArgs args)
        {
            Data.Init();
        }

        private void PlayerHooks_PlayerPostLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs e)
        {
            if (e.Player.HasPermission(Permissions.bypassssc))
            {
                return;
            }
            new Thread(() =>
            {
                Thread.Sleep(1000);
                NetMessage.SendData(147, -1, -1, null, e.Player.Index, 0);
                //切换到装备栏1
                var list = Player.Get(e.Player.Name);
                var now = new Player() { Name = e.Player.Name };
                if (list.Any())
                {
                    now = list[0];
                }
                else
                {
                    now.Insert();
                }
                for (var i = 0; i < 20; i++)
                {
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Armor0, now.Armor0[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0, now.Armor1[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0, now.Armor2[i]);
                }
                for (var i = 0; i < 10; i++)
                {
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Dye0, now.Dye0[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0, now.Dye1[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0, now.Dye2[i]);
                }
                Now.Add(e.Player.Name, now);
                //切换回原本的装备栏
                NetMessage.SendData(147, -1, -1, null, e.Player.Index, now.CurrentLoadoutIndex);
            })
            {
                IsBackground = true
            }.Start();
        }

        private void PlayerHooks_PlayerLogout(TShockAPI.Hooks.PlayerLogoutEventArgs e)
        {
            if (Now.ContainsKey(e.Player.Name))
            {
                Now[e.Player.Name].Update(new List<string>() { "Name" }, "Dye0", "Dye1", "Dye2", "Armor0", "Armor1", "Armor2", "CurrentLoadoutIndex");
                Now.Remove(e.Player.Name);
            }
            if (Source.ContainsKey(e.Player.Name))
            {
                //切换到装备栏1
                NetMessage.TrySendData(147, -1, -1, null, e.Player.Index, 0);
                var now = Source[e.Player.Name];
                for (var i = 0; i < 20; i++)
                {
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Armor0 + i, now.Armor0[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0 + i, now.Armor1[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0 + i, now.Armor2[i]);
                }
                for (var i = 0; i < 10; i++)
                {
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Dye0 + i, now.Dye0[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 + i, now.Dye1[i]);
                    SetPlayerInvSlot(e.Player, i + Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 + i, now.Dye2[i]);
                }
                Source.Remove(e.Player.Name);
            }
        }

        private void OnNetGetData(GetDataEventArgs args)
        {
            if (args.Msg.readBuffer[2] != 147)
                return;
            var player = TShock.Players[args.Msg.whoAmI];
            if (player.IsLoggedIn && !player.HasPermission(Permissions.bypassssc))
            {
                if (Now.ContainsKey(player.Name))
                    Now[player.Name].CurrentLoadoutIndex = args.Msg.readBuffer[4];
                player.TPlayer.CurrentLoadoutIndex = args.Msg.readBuffer[4];
            }
        }

        private void OnPlayerSlot(object? sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            try
            {
                if (Now.ContainsKey(e.Player.Name))
                {
                    if (e.Slot >= Terraria.ID.PlayerItemSlotID.Armor0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Armor0 + 20)
                    {
                        var data = Now[e.Player.Name];
                        var index = e.Slot - Terraria.ID.PlayerItemSlotID.Armor0;
                        var item = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                        switch (Now[e.Player.Name].CurrentLoadoutIndex)
                        {
                            case 0:
                                {
                                    data.Armor0[index] = item;
                                }
                                break;
                            case 1:
                                {
                                    data.Armor1[index] = item;
                                }
                                break;
                            case 2:
                                {
                                    data.Armor2[index] = item;
                                }
                                break;
                        }
                    }
                    else if (e.Slot >= Terraria.ID.PlayerItemSlotID.Dye0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Dye0 + 10)
                    {
                        var data = Now[e.Player.Name];
                        var index = e.Slot - Terraria.ID.PlayerItemSlotID.Dye0;
                        var item = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                        switch (Now[e.Player.Name].CurrentLoadoutIndex)
                        {
                            case 0:
                                {
                                    data.Dye0[index] = item;
                                }
                                break;
                            case 1:
                                {
                                    data.Dye1[index] = item;
                                }
                                break;
                            case 2:
                                {
                                    data.Dye2[index] = item;
                                }
                                break;
                        }
                    }
                }
                else
                //装备栏1
                if (e.Slot >= Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0 + 20)
                {
                    //初始化玩家
                    //开始记录原始装备栏数据
                    if (!Source.ContainsKey(e.Player.Name))
                    {
                        var data = new Player() { Name = e.Player.Name };
                        Source.Add(e.Player.Name, data);
                    }
                    Source[e.Player.Name].Armor0[e.Slot - Terraria.ID.PlayerItemSlotID.Loadout1_Armor_0] = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                }
                else if (e.Slot >= Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0 + 10)
                {
                    Source[e.Player.Name].Dye0[e.Slot - Terraria.ID.PlayerItemSlotID.Loadout1_Dye_0] = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                }
                //装备栏2
                else if (e.Slot >= Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0 + 20)
                {
                    Source[e.Player.Name].Armor1[e.Slot - Terraria.ID.PlayerItemSlotID.Loadout2_Armor_0] = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                }
                else if (e.Slot >= Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0 + 10)
                {
                    Source[e.Player.Name].Dye1[e.Slot - Terraria.ID.PlayerItemSlotID.Loadout2_Dye_0] = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                }
                //装备栏3
                else if (e.Slot >= Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0 + 20)
                {
                    Source[e.Player.Name].Armor2[e.Slot - Terraria.ID.PlayerItemSlotID.Loadout3_Armor_0] = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                }
                else if (e.Slot >= Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0 && e.Slot <= Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0 + 10)
                {
                    Source[e.Player.Name].Dye2[e.Slot - Terraria.ID.PlayerItemSlotID.Loadout3_Dye_0] = new Item() { netID = e.Type, stack = e.Stack, prefix = e.Prefix };
                }
            }
            catch { }
        }

        public static void SetPlayerInvSlot(TSPlayer player, int index)
        {
            SetPlayerInvSlot(player, index, new Item());
        }
        public static void SetPlayerInvSlot(int player, int index)
        {
            SetPlayerInvSlot(TShock.Players[player], index, new Item());
        }
        public static void SetPlayerInvSlot(int player, int index, Item item)
        {
            SetPlayerInvSlot(TShock.Players[player], index, item);
        }
        public static void SetPlayerInvSlot(TSPlayer player, int index, Item item)
        {
            if (item == null)
                item = new Item();
            using (MemoryStream data = new MemoryStream())
            {
                using (BinaryWriter wr = new BinaryWriter(data))
                {
                    wr.Write((short)11);
                    wr.Write((byte)5);
                    wr.Write((byte)player.Index);
                    wr.Write((short)index);
                    wr.Write((short)item.stack);
                    wr.Write((byte)item.prefix);
                    wr.Write((short)item.netID);
                }
                player.SendRawData(data.ToArray());
            }
        }
    }
}