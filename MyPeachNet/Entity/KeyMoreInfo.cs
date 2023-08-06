using MyPeach.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.breezee.MyPeachNet
{
    /**
     * @objectName: 键更多信息
     * @description: N或M-非空；R-替换；LI-整型列表；LS-字符列表
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/16 23:53
     * @history:
     *    2023/07/27 BreezeeHui 增加LI和LS中传入的为字符时，先去掉单引号，根据传入值以逗号分隔后，重新做值替换。listConvert中传入值为空时直接返回。
     *    2023/08/04 BreezeeHui 键设置增加优先使用配置项（F）的支持，即当一个键出现多次时，优先使用该配置内容。
     */
    public class KeyMoreInfo
    {
        /**
         * 可空（默认是）
         */
        public bool Nullable { get; set; } = true;
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsMust { 
            get { return !Nullable; }
            set { Nullable = !value;} 
        }
        public string InString { get; set; } = string.Empty;

        /// <summary>
        /// 是否优先使用的配置（默认否）
        /// </summary>
        public bool IsFirst{ get; set; } = false;
        public bool MustValueReplace { get; set; } = false;
        /**
         * 构建【键更多信息】对象
         * @param sKeyMore 键更多信息字符，例如：CITY_NAME:N
         * @return
         */
        public static KeyMoreInfo build(string sKeyMore, object objValue)
        {
            KeyMoreInfo moreInfo = new KeyMoreInfo();
            string[] arr = sKeyMore.Split(new char[] { ':','：' });//也支持中文冒号
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == 0) continue;
                string sOne = arr[i];
                if (string.IsNullOrEmpty(sOne)) continue;

                if (SqlKeyConfig.NOT_NULL.Equals(sOne) || SqlKeyConfig.IS_MUST.Equals(sOne))
                {
                    moreInfo.Nullable = false;//是否可空
                }
                else if (SqlKeyConfig.VALUE_REPLACE.Equals(sOne))
                {
                    moreInfo.MustValueReplace = true;//必须替换
                }
                else if (SqlKeyConfig.IS_FIRST.Equals(sOne))
                {
                    moreInfo.IsFirst = true;//是否优先使用本配置
                }
                else if (SqlKeyConfig.STRING_LIST.Equals(sOne))
                {
                    listConvert(objValue, moreInfo, true);
                }
                else if (SqlKeyConfig.INTEGE_LIST.Equals(sOne))
                {
                    listConvert(objValue, moreInfo, false);
                }
                //子配置项：未考虑好
                string[] arrChild = sOne.Split(new char[] { '-' });
                for (int j = 0; j < arrChild.Length; j++)
                {
                    string sOneItem = arrChild[j];
                }
            }
            return moreInfo;
        }

        /**
         * 转换SQL中的IN表单
         * @param objValue
         * @param moreInfo
         * @param stringFlag
         */
        private static void listConvert(object objValue, KeyMoreInfo moreInfo, bool stringFlag)
        {
            if(objValue== null) return;

            string sPreEnd = stringFlag ? "'" : string.Empty;
            string sCenter = stringFlag ? "','" : ",";

            //int数组或集合
            if (objValue is int[])
            {
                moreInfo.InString = sPreEnd + string.Join(sCenter, (string[])objValue) + sPreEnd;
                moreInfo.MustValueReplace = true;
                return;
            }
            else if (objValue is List<int>)
            {
                List<int> list = ((List<int>)objValue);
                moreInfo.InString = sPreEnd + string.Join(sCenter, list) + sPreEnd;
                moreInfo.MustValueReplace = true;
                return;
            }
            else if (objValue is string[])
            {
                moreInfo.InString = sPreEnd + string.Join(sCenter, (string[])objValue) + sPreEnd;
                moreInfo.MustValueReplace = true;
                return;
            }
            else if (objValue is List<string>)
            {
                List<string> list = ((List<string>)objValue);
                moreInfo.InString = sPreEnd + string.Join(sCenter, list) + sPreEnd;
                moreInfo.MustValueReplace = true;
                return;
            }
            else
            {
                string[] sValue = objValue.ToString().Replace("'","").Split(new char[] { ',','，'});
                moreInfo.InString = sPreEnd + string.Join(sCenter, sValue) + sPreEnd;
                moreInfo.MustValueReplace = true;
                return;
            }

        }

        #region 为了能直接复制过来的Java代码而增加的方法
        public string getInString()
        {
            return InString;
        }

        public bool isMustValueReplace()
        {
            return MustValueReplace;
        }

        public bool isNullable()
        {
            return Nullable;
        }

        #endregion

    }
}
