using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace FixSSC_Temp
{
    public class Item
    {
        public int netID { get; set; }
        public int stack { get; set; }
        public byte prefix { get; set; }
        public static implicit operator Item(Terraria.Item obj)
        {
            return new Item() { netID = obj.netID, stack = obj.stack };
        }
        public static implicit operator Terraria.Item(Item obj)
        {
            return new Item() { netID = obj.netID, stack = obj.stack };
        }
    }
    [TableName("ExtraArmors")]
    public class Player : Table<Player>
    {
        public string Name { get; set; } = "";
        public byte CurrentLoadoutIndex { get; set; }
        public Item[] Dye0 { get; set; } = new Item[10];
        public Item[] Dye1 { get; set; } = new Item[10];
        public Item[] Dye2 { get; set; } = new Item[10];
        public Item[] Armor0 { get; set; } = new Item[20];
        public Item[] Armor1 { get; set; } = new Item[20];
        public Item[] Armor2 { get; set; } = new Item[20];
        public static List<Player> Get(string name)
        {
            return Get(new Dictionary<string, object>() { { "Name", name } });
        }
    }
}
