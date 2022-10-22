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
        public override string ToString()
        {
            return $"{netID},{stack},{prefix}";
        }
    }
    [TableName("ExtraArmors")]
    public class Player : Table<Player>
    {
        public string Name { get; set; } = "";
        public byte CurrentLoadoutIndex { get; set; }
        public string Loadouts { get; set; } = "";
        [TableIgnore]
        public Item[] Dye0 { get; set; } = new Item[10];
        [TableIgnore]
        public Item[] Dye1 { get; set; } = new Item[10];
        [TableIgnore]
        public Item[] Dye2 { get; set; } = new Item[10];
        [TableIgnore]
        public Item[] Armor0 { get; set; } = new Item[20];
        [TableIgnore]
        public Item[] Armor1 { get; set; } = new Item[20];
        [TableIgnore]
        public Item[] Armor2 { get; set; } = new Item[20];
        public static Player Get(string name)
        {
            var list= Get(new Dictionary<string, object>() { { "Name", name } });
            if (list.Any())
            {
                var items = list[0].Loadouts.Split('~').Select(x => new Item() { netID = int.Parse(x.Split(',')[0]), stack = int.Parse(x.Split(',')[1]), prefix = byte.Parse(x.Split(',')[2]) });
                var res = list[0];
                res.Dye0 = items.Take(10).ToArray();
                res.Dye1 = items.Skip(10).Take(10).ToArray();
                res.Dye2 = items.Skip(20).Take(10).ToArray();
                res.Armor0 = items.Skip(30).Take(20).ToArray();
                res.Armor1 = items.Skip(50).Take(20).ToArray();
                res.Armor2 = items.Skip(70).Take(20).ToArray();
                return res;
            }
            else
            {
                return null;
            }
        }
        public void Save()
        {
            Insert();
        }
        public void UpdateThis()
        {
            var list=new List<Item>();
            list.AddRange(Dye0);
            list.AddRange(Dye1);
            list.AddRange(Dye2);
            list.AddRange(Armor0);
            list.AddRange(Armor1);
            list.AddRange(Armor2);
            Loadouts = String.Join("~", list.Select(x => x.ToString()));
            Update(new List<string>() { "Name" }, "Loadouts");
        }
    }
}
