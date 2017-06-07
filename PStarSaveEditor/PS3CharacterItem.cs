using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStarSaveEditor
{
    public class PS3CharacterItem
    {
        #region - Class Fields -
        private string name;
        private string speedLoc;
        private string nameLoc;
        private string levelLoc;
        private string maxHPLoc;
        private string maxTPLoc;
        private string curHPLoc;
        private string curTPLoc;
        private string dmgLoc;
        private string defLoc;
        private string expLoc;
        private string luckLoc;
        private string skillLoc;
        private string poisonLoc;
        private string itemCntLoc;
        #endregion

        #region - Class Properties -
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        public string SpeedLoc
        {
            get { return speedLoc; }
            set { speedLoc = value; }
        }
        public string NameLoc
        {
            get { return nameLoc; }
            set { nameLoc = value; }
        }
        public string LevelLoc
        {
            get { return levelLoc; }
            set { levelLoc = value; }
        }
        public string MaxHPLoc
        {
            get { return maxHPLoc; }
            set { maxHPLoc = value; }
        }
        public string MaxTPLoc
        {
            get { return maxTPLoc; }
            set { maxTPLoc = value; }
        }
        public string CurHPLoc
        {
            get { return curHPLoc; }
            set { curHPLoc = value; }
        }
        public string CurTPLoc
        {
            get { return curTPLoc; }
            set { curTPLoc = value; }
        }
        public string DmgLoc
        {
            get { return dmgLoc; }
            set { dmgLoc = value; }
        }
        public string DefLoc
        {
            get { return defLoc; }
            set { defLoc = value; }
        }
        public string ExpLoc
        {
            get { return expLoc; }
            set { expLoc = value; }
        }
        public string LuckLoc
        {
            get { return luckLoc; }
            set { luckLoc = value; }
        }
        public string SkillLoc
        {
            get { return skillLoc; }
            set { skillLoc = value; }
        }
        public string PoisonLoc
        {
            get { return poisonLoc; }
            set { poisonLoc = value; }
        }
        public string ItemCntLoc
        {
            get { return itemCntLoc; }
            set { itemCntLoc = value; }
        }
        #endregion

        #region - Class Constructor -
        public PS3CharacterItem(string name, string speedLoc, string nameLoc, string levelLoc, string maxHPLoc, string maxTPLoc, string curHPLoc, string curTPLoc, string dmgLoc, string defLoc, string expLoc, string luckLoc, string skillLoc, string poisonLoc, string itemCntLoc)
        {
            Name = name;
            SpeedLoc = speedLoc;
            NameLoc = nameLoc;
            LevelLoc = levelLoc;
            MaxHPLoc = maxHPLoc;
            MaxTPLoc = maxTPLoc;
            CurHPLoc = curHPLoc;
            CurTPLoc = curTPLoc;
            DmgLoc = dmgLoc;
            DefLoc = defLoc;
            ExpLoc = expLoc;
            LuckLoc = luckLoc;
            SkillLoc = skillLoc;
            PoisonLoc = poisonLoc;
            ItemCntLoc = itemCntLoc;
        }

        public PS3CharacterItem()
            : this(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        public PS3CharacterItem(PS3CharacterItem charItem)
        {
            Name = charItem.Name;
            SpeedLoc = charItem.SpeedLoc;
            NameLoc = charItem.NameLoc;
            LevelLoc = charItem.LevelLoc;
            MaxHPLoc = charItem.MaxHPLoc;
            MaxTPLoc = charItem.MaxTPLoc;
            CurHPLoc = charItem.CurHPLoc;
            CurTPLoc = charItem.CurTPLoc;
            DmgLoc = charItem.DmgLoc;
            DefLoc = charItem.DefLoc;
            ExpLoc = charItem.ExpLoc;
            LuckLoc = charItem.LuckLoc;
            SkillLoc = charItem.SkillLoc;
            PoisonLoc = charItem.PoisonLoc;
            ItemCntLoc = charItem.ItemCntLoc;
        }
        #endregion
    }
}
