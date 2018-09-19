using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStarSaveEditor
{
    public class PS1CharacterItem
    {
        #region - Class Fields -
        private string name;
        private string experienceLoc;
        private string levelLoc;
        private string currentHPLoc;
        private string maxHPLoc;
        private string currentMPLoc;
        private string maxMPLoc;
        private string attackLoc;
        private string defenseLoc;
        private string equippedWeaponLoc;
        private string equippedArmorLoc;
        private string equippedShieldLoc;
        #endregion

        #region - Class Properties -
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string ExperienceLoc
        {
            get { return experienceLoc; }
            set { experienceLoc = value; }
        }
        public string LevelLoc
        {
            get { return levelLoc; }
            set { levelLoc = value; }
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
        public string CurrentMPLoc
        {
            get { return currentMPLoc; }
            set { currentMPLoc = value; }
        }
        public string MaxMPLoc
        {
            get { return maxMPLoc; }
            set { maxMPLoc = value; }
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
        public string EquippedWeaponLoc
        {
            get { return equippedWeaponLoc; }
            set { equippedWeaponLoc = value; }
        }
        public string EquippedArmorLoc
        {
            get { return equippedArmorLoc; }
            set { equippedArmorLoc = value; }
        }
        public string EquippedShieldLoc
        {
            get { return equippedShieldLoc; }
            set { equippedShieldLoc = value; }
        }
        #endregion

        #region - Class Constructors -
        public PS1CharacterItem(string name, 
            string experienceLoc, 
            string levelLoc, 
            string currentHPLoc, 
            string maxHPLoc,
            string currentMPLoc,
            string maxMPLoc,
            string attackLoc, 
            string defenseLoc, 
            string equippedWeaponLoc, 
            string equippedArmorLoc, 
            string equippedShieldLoc)
        {
            Name = name;
            ExperienceLoc = experienceLoc;
            LevelLoc = levelLoc;
            CurrentHPLoc = currentHPLoc;
            MaxHPLoc = maxHPLoc;
            CurrentMPLoc = currentMPLoc;
            MaxMPLoc = maxMPLoc;
            AttackLoc = attackLoc;
            DefenseLoc = defenseLoc;
            EquippedWeaponLoc = equippedWeaponLoc;
            EquippedArmorLoc = equippedArmorLoc;
            EquippedShieldLoc = equippedShieldLoc;
        }

        public PS1CharacterItem()
            : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public PS1CharacterItem(PS1CharacterItem characterItem)
        {
            Name = characterItem.Name;
            ExperienceLoc = characterItem.ExperienceLoc;
            LevelLoc = characterItem.LevelLoc;
            CurrentHPLoc = characterItem.CurrentHPLoc;
            MaxHPLoc = characterItem.MaxHPLoc;
            CurrentMPLoc = characterItem.CurrentMPLoc;
            MaxMPLoc = characterItem.MaxMPLoc;
            AttackLoc = characterItem.AttackLoc;
            DefenseLoc = characterItem.DefenseLoc;
            EquippedWeaponLoc = characterItem.equippedWeaponLoc;
            EquippedArmorLoc = characterItem.EquippedArmorLoc;
            EquippedShieldLoc = characterItem.EquippedShieldLoc;
        }
        #endregion
    }
}
