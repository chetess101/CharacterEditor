using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CharacterEditor.Character
{
	public class Item : ICharacterData
	{
		public const int AttributeCount = 32;
		public const int PrefixDivisor = 10;
		public const int ModelDivisor = 11;
		public const int EffectDivisor = 21;

		public byte Type;
		public byte Subtype;
		//public short ActualModifier; // TODO Hack, should build modifier from its fragments
		public byte RecipeType;
		public byte Rarity;
		public byte Material;
		public ItemFlags Flags;
		public short Level;

		// Modifier subparts
		public int PrefixId;
		public int ModelId;
		public int EffectId;

		public List<ItemAttribute> Attributes;

		public short Modifier
		{
			get
			{
				int modifier, t;
				ChineseRemainder.Solve(PrefixDivisor, ModelDivisor, EffectDivisor, PrefixId, ModelId, EffectId, out modifier, out t);

				return (short)modifier;
			}

			private set
			{
				PrefixId = value % PrefixDivisor;
				ModelId = value % ModelDivisor;
				EffectId = value % EffectDivisor;
			}
		}

		public string FriendlyName
		{
			get
			{
				try
				{
					// TODO Should probably clean this up
					string format = "{0}";

					string ownerName = NameGenerator.Generate(Modifier, (Modifier * 7) % 11);
					string itemName = string.Empty;

					if (Type == (int)Constants.ItemType.Recipes)
					{
						itemName += Constants.ItemSubtypes[RecipeType][Subtype] + " ";
						itemName += Constants.ItemSubtypes[(int)Constants.ItemType.Recipes][Subtype];
					}
					else
						itemName += Constants.ItemSubtypes[Type][Subtype];

					if (Material != 0)
						itemName = Constants.ItemMaterialNames[Material] + " " + itemName;

					if (Modifier != 0)
						format = Constants.ItemModifiers[Rarity][(Modifier - 1) % 10];

					return Rarity < 3 ? string.Format(format, itemName) : string.Format(format, itemName, ownerName);
				}
				catch (Exception)
				{
					return "ERROR";
				}
			}
		}

		[Flags]
		public enum ItemFlags : byte
		{
			Adapted = 0x01
		}

		public Item()
		{
			Attributes = new List<ItemAttribute>();
		}

		public void Read(BinaryReader reader)
		{
			Type = reader.ReadByte();
			Subtype = reader.ReadByte();
			reader.Skip(2);
			Modifier = reader.ReadInt16();
			reader.Skip(2);
			RecipeType = reader.ReadByte();
			reader.Skip(3);
			Rarity = reader.ReadByte();
			Material = reader.ReadByte();
			Flags = (ItemFlags)reader.ReadByte();
			reader.Skip(1);
			Level = reader.ReadInt16();
			reader.Skip(2);

			for (int i = 0; i < AttributeCount; ++i)
			{
				ItemAttribute attribute = new ItemAttribute();
				attribute.Read(reader);

				Attributes.Add(attribute);
			}

			// AttributesUsed is calculated on write
			reader.Skip(4);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Type);
			writer.Write(Subtype);
			writer.Skip(2);
			writer.Write(Modifier);
			writer.Skip(2);
			writer.Write(RecipeType);
			writer.Skip(3);
			writer.Write(Rarity);
			writer.Write(Material);
			writer.Write((byte)Flags);
			writer.Skip(1);
			writer.Write(Level);
			writer.Skip(2);

			foreach (ItemAttribute attribute in Attributes)
				attribute.Write(writer);

			writer.Write(Attributes.Count(attr => attr.Used));
		}
	}
}