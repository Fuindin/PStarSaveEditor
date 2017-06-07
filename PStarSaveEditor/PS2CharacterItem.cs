using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStarSaveEditor
{
    public class PS2CharacterItem
    {
        #region - Class Fields -
        private string name;
        private string currentHPLoc;
        private string maxHPLoc;
        private string currentTPLoc;
        private string maxTPLoc;
        private string levelLoc;
        private string experienceLoc;
        private string strengthLoc;
        private string mentalLoc;
        private string agilityLoc;
        private string luckLoc;
        private string dexterityLoc;
        private string attackLoc;
        private string defenseLoc;
        #endregion

        #region - Class Properties -
        public string Name
        {
            get { return name; }
            set { name = value; }
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
        public string LevelLoc
        {
            get { return levelLoc; }
            set { levelLoc = value; }
        }
        public string ExperienceLoc
        {
            get { return experienceLoc; }
            set { experienceLoc = value; }
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
        public string LuckLoc
        {
            get { return luckLoc; }
            set { luckLoc = value; }
        }
        public string DexterityLoc
        {
            get { return dexterityLoc; }
            set { dexterityLoc = value; }
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

        #region - Class Constructor -
        public PS2CharacterItem(string name, string currentHPLoc, string maxHPLoc, string currentTPLoc, string maxTPLoc, string levelLoc, string experienceLoc, string strengthLoc, string mentalLoc, string agilityLoc, string luckLoc, string dexterityLoc, string attackLoc, string defenseLoc)
        {
            Name = name;
            CurrentHPLoc = currentHPLoc;
            MaxHPLoc = maxHPLoc;
            CurrentTPLoc = currentTPLoc;
            MaxTPLoc = maxTPLoc;
            LevelLoc = levelLoc;
            ExperienceLoc = experienceLoc;
            StrengthLoc = strengthLoc;
            MentalLoc = mentalLoc;
            AgilityLoc = agilityLoc;
            LuckLoc = luckLoc;
            DexterityLoc = dexterityLoc;
            AttackLoc = attackLoc;
            DefenseLoc = defenseLoc;
        }

        public PS2CharacterItem()
            : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public PS2CharacterItem(PS2CharacterItem charItem)
        {
            Name = charItem.Name;
            CurrentHPLoc = charItem.CurrentHPLoc;
            MaxHPLoc = charItem.MaxHPLoc;
            CurrentTPLoc = charItem.CurrentTPLoc;
            MaxTPLoc = charItem.MaxTPLoc;
            LevelLoc = charItem.LevelLoc;
            ExperienceLoc = charItem.ExperienceLoc;
            StrengthLoc = charItem.StrengthLoc;
            MentalLoc = charItem.MentalLoc;
            AgilityLoc = charItem.AgilityLoc;
            LuckLoc = charItem.LuckLoc;
            DexterityLoc = charItem.DexterityLoc;
            AttackLoc = charItem.AttackLoc;
            DefenseLoc = charItem.DefenseLoc;
        }
        #endregion
    }
}
