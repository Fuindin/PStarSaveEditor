using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStarSaveEditor
{
    public class PS4CharacterItem
    {
        #region - Class Fields -
        private string name;
        private string levelLoc;
        private string expLoc;
        private string currentHPLoc;
        private string maxHPLoc;
        private string currentTPLoc;
        private string maxTPLoc;
        private string strengthLoc;
        private string mentalLoc;
        private string agilityLoc;
        private string dexterityLoc;
        private string weaponSlot1Loc;
        private string weaponSlot2Loc;
        private string helmetLoc;
        private string armorLoc;
        private string attackLoc;
        private string defenseLoc;
        #endregion

        #region - Class Properties -
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string LevelLoc
        {
            get { return levelLoc; }
            set { levelLoc = value; }
        }
        public string ExpLoc
        {
            get { return expLoc; }
            set { expLoc = value; }
        }
        public string CurrentHPLoc
        {
            get { return currentHPLoc; }
            set { currentHPLoc = value; }
        }
        public string MaxHPLoc
        {
            get { return maxHPLoc; }
            set { maxHPLoc = value; }
        }
        public string CurrentTPLoc
        {
            get { return currentTPLoc; }
            set { currentTPLoc = value; }
        }
        public string MaxTPLoc
        {
            get { return maxTPLoc; }
            set { maxTPLoc = value; }
        }
        public string StrengthLoc
        {
            get { return strengthLoc; }
            set { strengthLoc = value; }
        }
        public string MentalLoc
        {
            get { return mentalLoc; }
            set { mentalLoc = value; }
        }
        public string AgilityLoc
        {
            get { return agilityLoc; }
            set { agilityLoc = value; }
        }
        public string DexterityLoc
        {
            get { return dexterityLoc; }
            set { dexterityLoc = value; }
        }
        public string WeaponSlot1Loc
        {
            get { return weaponSlot1Loc; }
            set { weaponSlot1Loc = value; }
        }
        public string WeaponSlot2Loc
        {
            get { return weaponSlot2Loc; }
            set { weaponSlot2Loc = value; }
        }
        public string HelmetLoc
        {
            get { return helmetLoc; }
            set { helmetLoc = value; }
        }
        public string ArmorLoc
        {
            get { return armorLoc; }
            set { armorLoc = value; }
        }
        public string AttackLoc
        {
            get { return attackLoc; }
            set { attackLoc = value; }
        }
        public string DefenseLoc
        {
            get { return defenseLoc; }
            set { defenseLoc = value; }
        }
        #endregion

        #region - Class Constructors -
        public PS4CharacterItem(string name, string levelLoc, string expLoc, string currentHPLoc, string maxHPLoc, string currentTPLoc, string maxTPLoc, string strengthLoc, string mentalLoc, string agilityLoc, string dexterityLoc, string weaponSlot1Loc, string weaponSlot2Loc, string helmetLoc, string armorLoc, string attackLoc, string defenseLoc)
        {
            Name = name;
            LevelLoc = levelLoc;
            ExpLoc = expLoc;
            CurrentHPLoc = currentHPLoc;
            MaxHPLoc = maxHPLoc;
            CurrentTPLoc = currentTPLoc;
            MaxTPLoc = maxTPLoc;
            StrengthLoc = strengthLoc;
            MentalLoc = mentalLoc;
            AgilityLoc = agilityLoc;
            DexterityLoc = dexterityLoc;
            WeaponSlot1Loc = weaponSlot1Loc;
            WeaponSlot2Loc = weaponSlot2Loc;
            HelmetLoc = helmetLoc;
            ArmorLoc = armorLoc;
            AttackLoc = attackLoc;
            DefenseLoc = defenseLoc;
        }

        public PS4CharacterItem() : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }
        #endregion
    }
}
