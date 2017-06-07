using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStarSaveEditor
{
    public class PSItem
    {
        #region - Class Fields -
        private string itemID;
        private string itemName;
        #endregion

        #region - Class Properties -
        public string ItemID
        {
            get { return itemID; }
            set { itemID = value; }
        }
        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }
        #endregion

        #region - Class Constructors -
        public PSItem(string itemID, string itemName)
        {
            ItemID = itemID;
            ItemName = itemName;
        }

        public PSItem() : this(string.Empty, string.Empty)
        {

        }
        #endregion
    }
}
