﻿using MagicMongoDBTool.Module;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MagicMongoDBTool
{
    public partial class frmShardingConfig : System.Windows.Forms.Form
    {
        /// <summary>
        /// 初期化
        /// </summary>
        public frmShardingConfig()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Mongo服务器
        /// </summary>
        private MongoServer _prmSvr;
        /// <summary>
        /// Tag和Set的字典
        /// </summary>
        private Dictionary<String, String> TagSet = new Dictionary<string, string>();
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmAddSharding_Load(object sender, EventArgs e)
        {
            if (!SystemManager.IsUseDefaultLanguage)
            {
                cmdClose.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Close);
                cmdAddHost.Text = SystemManager.mStringResource.GetText(StringResource.TextType.AddConnection_Region_AddHost);
                cmdRemoveHost.Text = SystemManager.mStringResource.GetText(StringResource.TextType.AddConnection_Region_RemoveHost);
                lblReplHost.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Host);
                lblReplPort.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Port);

                this.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_Title);
                tabAddSharding.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_AddSharding);
                lblMainReplsetName.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_ReplsetName);
                cmdAddSharding.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.Common_Add);
                chkAdvance.Text = SystemManager.mStringResource.GetText(StringResource.TextType.Common_Advance_Option);
                tabShardingConfig.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_EnableSharding);
                lblDBName.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_DBName);
                lblCollection.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_CollectionName);
                lblIndexLName.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_FieldName);
                cmdEnableCollectionSharding.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_Action_CollectionSharding);
                cmdEnableDBSharding.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.ShardingConfig_Action_DBSharding);
            }
            _prmSvr = SystemManager.GetCurrentServer();
            MongoDatabase mongoDB = _prmSvr.GetDatabase(MongoDBHelper.DATABASE_NAME_CONFIG);
            MongoCollection mongoCol = mongoDB.GetCollection("databases");
            foreach (var item in mongoCol.FindAllAs<BsonDocument>())
            {
                if (item.GetValue(MongoDBHelper.KEY_ID) != MongoDBHelper.DATABASE_NAME_ADMIN)
                {
                    cmbDataBase.Items.Add(item.GetValue(MongoDBHelper.KEY_ID));
                    cmbShardKeyDB.Items.Add(item.GetValue(MongoDBHelper.KEY_ID));
                };
            }
            foreach (var lst in MongoDBHelper.GetShardInfo(_prmSvr, MongoDBHelper.KEY_ID))
            {
                lstSharding.Items.Add(lst.Value);
            }
            mongoCol = mongoDB.GetCollection("shards");
            cmbTagList.Items.Clear();
            foreach (BsonDocument mShard in mongoCol.FindAllAs<BsonDocument>())
            {
                if (mShard.Contains("tags"))
                {
                    foreach (BsonValue tag in mShard.GetElement("tags").Value.AsBsonArray)
                    {
                        //严格意义上说，不应该在同一个路由里面出现两个同名的标签。
                        if (!TagSet.ContainsKey(tag.ToString()))
                        {
                            TagSet.Add(tag.ToString(), mShard.GetElement(MongoDBHelper.KEY_ID).Value.ToString());
                            cmbTagList.Items.Add(mShard.GetElement(MongoDBHelper.KEY_ID).Value.ToString() + "." + tag.ToString());
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 增加Shard的高级选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAdvance_CheckedChanged(object sender, EventArgs e)
        {
            grpAdvanced.Enabled = chkAdvance.Checked;
        }
        /// <summary>
        /// 添加HostList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAddHost_Click(object sender, EventArgs e)
        {
            String strHost = String.Empty;
            strHost = txtReplHost.Text;
            if (NumReplPort.Value != 0)
            {
                strHost += ":" + NumReplPort.Value.ToString();
            }
            lstHost.Items.Add(strHost);
            cmdAddSharding.Enabled = true;
        }
        /// <summary>
        /// 移除HostList
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdRemoveHost_Click(object sender, EventArgs e)
        {
            lstHost.Items.Remove(lstHost.SelectedItem);
            if (lstHost.Items.Count == 0)
            {
                cmdAddSharding.Enabled = false;
            }
        }
        /// <summary>
        /// 增加分片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdAddSharding_Click(object sender, EventArgs e)
        {
            List<String> lstAddress = new List<String>();
            foreach (String item in lstHost.Items)
            {
                lstAddress.Add(item.Trim());
            }
            List<CommandResult> Resultlst = new List<CommandResult>();
            CommandResult Result;
            if (chkAdvance.Checked)
            {
                Result = MongoDBHelper.AddSharding(_prmSvr, this.txtReplsetName.Text, lstAddress, txtName.Text, NumMaxSize.Value);
            }
            else
            {
                Result = MongoDBHelper.AddSharding(_prmSvr, this.txtReplsetName.Text, lstAddress, String.Empty, 0);
            }
            Resultlst.Add(Result);
            MyMessageBox.ShowMessage("Add Sharding", "Result:" + (Result.Ok ? "OK" : "Fail"), MongoDBHelper.ConvertCommandResultlstToString(Resultlst));
            lstSharding.Items.Clear();
            foreach (var lst in MongoDBHelper.GetShardInfo(_prmSvr, "_id"))
            {
                lstSharding.Items.Add(lst.Value);
            }
            lstHost.Items.Clear();
        }
        /// <summary>
        /// 数据库切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbDataBase_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                MongoDatabase mongoDB = _prmSvr.GetDatabase(cmbDataBase.Text);
                cmbCollection.Items.Clear();
                cmbCollection.Text = String.Empty;
                foreach (var item in mongoDB.GetCollectionNames())
                {
                    cmbCollection.Items.Add(item);
                }
                cmbIndexList.Items.Clear();
                lstExistShardTag.Items.Clear();
            }
            catch (Exception ex)
            {
                SystemManager.ExceptionDeal(ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbShardKeyDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                MongoDatabase mongoDB = _prmSvr.GetDatabase(cmbShardKeyDB.Text);
                cmbShardKeyCol.Items.Clear();
                cmbShardKeyCol.Text = String.Empty;
                foreach (var item in mongoDB.GetCollectionNames())
                {
                    cmbShardKeyCol.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                SystemManager.ExceptionDeal(ex);
            }
        }

        /// <summary>
        /// 数据集变换时，实时更新主键列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmbCollection_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                MongoDatabase mongoDB = _prmSvr.GetDatabase(cmbDataBase.Text);
                cmbIndexList.Items.Clear();
                cmbIndexList.Text = String.Empty;
                //Sharding Must Index
                foreach (var Indexitem in mongoDB.GetCollection(cmbCollection.Text).GetIndexes())
                {
                    cmbIndexList.Items.Add(Indexitem.Name);
                }
                cmbIndexList.Text = cmbIndexList.Items[0].ToString();
                //Tag和数据集绑定，从系统数据集tags里面读取tag的信息
                MongoDatabase mongoDBConfig = _prmSvr.GetDatabase(MongoDBHelper.DATABASE_NAME_CONFIG);
                MongoCollection mongoCol = mongoDBConfig.GetCollection("tags");
                lstExistShardTag.Items.Clear();
                lstExistShardTag.Columns.Add("Tag");
                lstExistShardTag.Columns.Add("NameSpace");
                lstExistShardTag.Columns.Add("Min");
                lstExistShardTag.Columns.Add("Max");
                foreach (BsonDocument tags in mongoCol.FindAllAs<BsonDocument>())
                {
                    if (tags.GetElement("ns").Value.ToString() != BsonUndefined.Value.ToString())
                    {
                        if (tags.GetElement("ns").Value.ToString() == cmbDataBase.Text + "." + cmbCollection.Text)
                        {
                            ListViewItem ListItem = new ListViewItem(TagSet[tags.GetElement("tag").Value.ToString()] + "." + tags.GetElement("tag").Value.ToString());
                            ListItem.SubItems.Add(tags.GetElement("ns").Value.ToString());
                            ListItem.SubItems.Add(tags.GetElement("min").Value.ToString());
                            ListItem.SubItems.Add(tags.GetElement("max").Value.ToString());
                            lstExistShardTag.Items.Add(ListItem);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SystemManager.ExceptionDeal(ex);
            }
        }
        /// <summary>
        /// 分片配置(数据库)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdEnableSharding_Click(object sender, EventArgs e)
        {
            List<CommandResult> Resultlst = new List<CommandResult>();
            Resultlst.Add(MongoDBHelper.EnableSharding(_prmSvr, cmbDataBase.Text));
            MyMessageBox.ShowMessage("EnableSharding", "Result", MongoDBHelper.ConvertCommandResultlstToString(Resultlst));
        }
        /// <summary>
        /// 分片配置(数据集)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdEnableCollectionSharding_Click(object sender, EventArgs e)
        {
            List<CommandResult> Resultlst = new List<CommandResult>();
            GetIndexesResult Result = _prmSvr.GetDatabase(cmbDataBase.Text).GetCollection(cmbCollection.Text).GetIndexes();
            BsonDocument IndexDoc = Result[cmbIndexList.SelectedIndex].Key;
            Resultlst.Add(MongoDBHelper.ShardCollection(_prmSvr, cmbDataBase.Text + "." + cmbCollection.Text, IndexDoc));
            MyMessageBox.ShowMessage("EnableSharding", "Result", MongoDBHelper.ConvertCommandResultlstToString(Resultlst));
        }
        /// <summary>
        /// 移除Sharding
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdRemoveSharding_Click(object sender, EventArgs e)
        {
            foreach (String item in lstSharding.SelectedItems)
            {
                List<CommandResult> Resultlst = new List<CommandResult>();
                Resultlst.Add(MongoDBHelper.RemoveSharding(_prmSvr, item));
                MyMessageBox.ShowMessage("Remove Sharding", "Result", MongoDBHelper.ConvertCommandResultlstToString(Resultlst));
            }
            lstSharding.Items.Clear();
            foreach (var lst in MongoDBHelper.GetShardInfo(_prmSvr, "_id"))
            {
                lstSharding.Items.Add(lst.Value);
            }
        }
        /// <summary>
        /// 为Sharding增加Tag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAddShardTag_Click(object sender, EventArgs e)
        {
            List<CommandResult> Resultlst = new List<CommandResult>();
            Resultlst.Add(MongoDBHelper.AddShardTag(_prmSvr, txtShardName.Text, txtTagShard.Text));
            MyMessageBox.ShowMessage("Add Shard Tag", "Result", MongoDBHelper.ConvertCommandResultlstToString(Resultlst));
        }
        /// <summary>
        /// Add Tag Range
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdaddTagRange_Click(object sender, EventArgs e)
        {
            List<CommandResult> Resultlst = new List<CommandResult>();
            Resultlst.Add(MongoDBHelper.AddTagRange(_prmSvr, cmbShardKeyDB.Text + "." + cmbShardKeyCol.Text, ctlBsonValueShardKeyFrom.getValue(),
                                                    ctlBsonValueShardKeyTo.getValue(), cmbTagList.Text.Split(".".ToCharArray())[1]));
            MyMessageBox.ShowMessage("Add Shard Tag", "Result", MongoDBHelper.ConvertCommandResultlstToString(Resultlst));

        }
        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
