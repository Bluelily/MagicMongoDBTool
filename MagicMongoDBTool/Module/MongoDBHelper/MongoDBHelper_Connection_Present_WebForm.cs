﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TreeViewColumnsProject;

namespace MagicMongoDBTool.Module
{
    public static partial class MongoDBHelper
    {
        #region"展示数据库结构 WebForm"
        /// <summary>
        /// 获取JSON
        /// </summary>
        /// <param name="ConnectionName"></param>
        /// <returns></returns>
        public static String GetConnectionzTreeJSON()
        {
            TreeView tree = new TreeView();
            FillConnectionToTreeView(tree);
            return ConvertTreeViewTozTreeJson(tree);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RootName"></param>
        /// <param name="doc"></param>
        /// <param name="IsOpen"></param>
        /// <returns></returns>
        public static String ConvertBsonTozTreeJson(String RootName, BsonDocument doc, Boolean IsOpen)
        {
            TreeViewColumns trvStatus = new TreeViewColumns();
            MongoDBHelper.FillDataToTreeView(RootName, trvStatus, doc);
            if (IsOpen)
            {
                trvStatus.TreeView.Nodes[0].Expand();
            }
            return ConvertTreeViewTozTreeJson(trvStatus.TreeView);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static String ConvertTreeViewTozTreeJson(TreeView tree)
        {
            BsonArray array = new BsonArray();
            foreach (TreeNode item in tree.Nodes)
            {
                array.Add(ConvertTreeNodeTozTreeBsonDoc(item));
            }
            return array.ToJson(SystemManager.JsonWriterSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SubNode"></param>
        /// <returns></returns>
        private static BsonDocument ConvertTreeNodeTozTreeBsonDoc(TreeNode SubNode)
        {
            BsonDocument SingleNode = new BsonDocument();
            SingleNode.Add("name", SubNode.Text + GetTagText(SubNode));
            if (SubNode.Nodes.Count == 0)
            {
                SingleNode.Add("icon", "MainTreeImage" + String.Format("{0:00}", SubNode.ImageIndex) + ".png");
            }
            else
            {
                BsonArray ChildrenList = new BsonArray();
                foreach (TreeNode item in SubNode.Nodes)
                {
                    ChildrenList.Add(ConvertTreeNodeTozTreeBsonDoc(item));
                }
                SingleNode.Add("children", ChildrenList);
                SingleNode.Add("icon", "MainTreeImage" + String.Format("{0:00}", SubNode.ImageIndex) + ".png");
            }
            if (SubNode.IsExpanded)
            {
                SingleNode.Add("open", "true");
            }
            if (SubNode.Tag != null)
            {
                SingleNode.Add("click", "ShowData('" + SystemManager.GetTagType(SubNode.Tag.ToString()) + "','" + SystemManager.GetTagData(SubNode.Tag.ToString()) + "')");
            }
            return SingleNode;
        }
        /// <summary>
        /// 展示数据值和类型
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static string GetTagText(TreeNode node)
        {
            string strColumnText = String.Empty;
            BsonElement Element = node.Tag as BsonElement;
            if (Element != null && !Element.Value.IsBsonDocument && !Element.Value.IsBsonArray)
            {
                strColumnText = ":" + Element.Value.ToString();
                strColumnText += "[" + Element.Value.GetType().Name.Substring(4) + "]";
            }
            return strColumnText;
        }
        #endregion
    }
}
