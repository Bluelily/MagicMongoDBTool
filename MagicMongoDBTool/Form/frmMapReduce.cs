﻿using MagicMongoDBTool.Module;
using MongoDB.Bson;
using MongoDB.Driver;
using System;

namespace MagicMongoDBTool
{
    public partial class frmMapReduce : System.Windows.Forms.Form
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public frmMapReduce()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 数据集
        /// </summary>
        private MongoCollection _mongocol = SystemManager.GetCurrentCollection();
        /// <summary>
        /// 载入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void frmMapReduce_Load(object sender, EventArgs e)
        {
            if (!SystemManager.IsUseDefaultLanguage)
            {
                ctlMapFunction.Title = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.MapReduce_MapFunction);
                ctlReduceFunction.Title = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.MapReduce_ReduceFunction);
                lblResult.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.MapReduce_Result);
                cmdRun.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.MapReduce_Run);
                cmdClose.Text = SystemManager.mStringResource.GetText(MagicMongoDBTool.Module.StringResource.TextType.Common_Close);
            }
            ctlMapFunction.Context = 
@"function Map(){
    emit(this.Age,1);
}";
            ctlReduceFunction.Context =
@"function Reduce(key, arr_values) {
     var total = 0;
     for(var i in arr_values){
         temp = arr_values[i];
         total += temp;
     }
     return total;
}";
        }
        /// <summary>
        /// 运行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdRun_Click(object sender, EventArgs e)
        {
            BsonJavaScript map = new BsonJavaScript(ctlMapFunction.Context);
            BsonJavaScript reduce = new BsonJavaScript(ctlReduceFunction.Context);
            //TODO:这里可能会超时，失去响应
            //需要设置SocketTimeOut
            MapReduceResult mMapReduceResult = _mongocol.MapReduce(map, reduce);
            MongoDBHelper.FillDataToTreeView("MapReduce Result", trvResult, mMapReduceResult.Response);
            trvResult.DatatreeView.BeginUpdate();
            trvResult.DatatreeView.ExpandAll();
            trvResult.DatatreeView.EndUpdate();
        }
        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cmdClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
