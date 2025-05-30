using Server.Gumps;

namespace Server.Items
{
	[FlipableAttribute( 0x6721, 0x6722 )]
	public class DeathKnightSpellbook : Spellbook
	{
		public override string DefaultDescription{ get{ return "This vile book can contain magic used by death knights. Fillings its pages can only be achieved by finding the resting places of long dead death knights."; } }

		[CommandProperty( AccessLevel.GameMaster )]
		public Mobile Owner { get; set; }

		public override SpellbookType SpellbookType{ get{ return SpellbookType.DeathKnight; } }
		public override int BookOffset{ get{ return 750; } }
		public override int BookCount{ get{ return 15; } }

		[Constructable]
		public DeathKnightSpellbook() : this( 0, null )
		{
		}

		[Constructable]
		public DeathKnightSpellbook( ulong content, Mobile gifted ) : base( content, 0x6721 )
		{
			Owner = gifted;

			string sEvil = "Evil";
			switch ( Utility.RandomMinMax( 0, 7 ) ) 
			{
				case 0: sEvil = "Evil";			break;
				case 1: sEvil = "Vile";			break;
				case 2: sEvil = "Sinister";		break;
				case 3: sEvil = "Wicked";		break;
				case 4: sEvil = "Corrupt";		break;
				case 5: sEvil = "Hateful";		break;
				case 6: sEvil = "Malevolent";	break;
				case 7: sEvil = "Nefarious";	break;
			}

			switch ( Utility.RandomMinMax( 1, 2 ) ) 
			{
				case 1: this.Name = "Kas' Book of " + sEvil + " Knights";	break;
				case 2: this.Name = "Kas' Tome of " + sEvil + " Knights";	break;
			}
		}

        public override bool CanEquip(Mobile from)
        {
			if (from != Owner)
			{
				from.SendLocalizedMessage( 1112589 ); // This does not belong to you! Find your own!
				return false;
			}

            return base.CanEquip(from);
        }

        public override void OnDoubleClick( Mobile from )
		{
			Container pack = from.Backpack;

			if ( Owner != from )
			{
				from.SendMessage( "These pages appears as scribbles to you." );
			}
			else if ( Parent == from || ( pack != null && Parent == pack ) )
			{
				from.SendSound( 0x55 );
				from.CloseGump( typeof( DeathKnightSpellbookGump ) );
				from.SendGump( new DeathKnightSpellbookGump( from, this, 1 ) );
			}
			else from.SendLocalizedMessage(500207); // The spellbook must be in your backpack (and not in a container within) to open.
		}

        public override void AddNameProperties(ObjectPropertyList list)
		{
            base.AddNameProperties(list);
			if ( Owner != null ){ list.Add( 1070722, "For " + Owner.Name + "" ); }
        }

		public DeathKnightSpellbook( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 ); // version
			writer.Write( (Mobile)Owner);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
			Owner = reader.ReadMobile();

			if ( ItemID != 0x6721 && ItemID != 0x6722 )
				ItemID = 0x6721;
		}
	}
}
