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
     * @description: 目前暂时只有一个N:非空
     * @author: guohui.huang
     * @email: guo7892000@126.com
     * @wechat: BreezeeHui
     * @date: 2022/4/16 23:53
     */
    public class KeyMoreInfo
    {
        /**
         * 可空（默认是）
         */
        public bool Nullable { get; set; } = true;
        public string StringList { get; set; } = string.Empty;

        /**
         * 构建【键更多信息】对象
         * @param sKeyMore 键更多信息字符，例如：CITY_NAME:N
         * @return
         */
        public static KeyMoreInfo build(string sKeyMore, object objValue)
        {
            KeyMoreInfo moreInfo = new KeyMoreInfo();
            string[] arr = sKeyMore.Split(":");
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == 0) continue;
                string sOne = arr[i];
                if (string.IsNullOrEmpty(sOne)) continue;

                if ("N".Equals(sOne))
                {
                    moreInfo.Nullable = false;
                }
                else if ("LS".Equals(sOne))
                {
                    listConvert(objValue, moreInfo, true);
                }
                else if ("LI".Equals(sOne))
                {
                    listConvert(objValue, moreInfo, false);
                }

                string[] arrChild = sOne.Split("-");
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
            string sPreEnd = stringFlag ? "'" : string.Empty;
            string sCenter = stringFlag ? "','" : ",";

            //int数组或集合
            if (objValue is int[])
            {
                moreInfo.StringList = sPreEnd + string.Join(sCenter, (string[])objValue) + sPreEnd;
                return;
            }
            else if (objValue is List<int>)
            {
                List<int> list = ((List<int>)objValue);
                moreInfo.StringList = sPreEnd + string.Join(sCenter, list) + sPreEnd;
                return;
            }
            else if (objValue is string[])
            {
                moreInfo.StringList = sPreEnd + string.Join(sCenter, (string[])objValue) + sPreEnd;
                return;
            }
            else if (objValue is List<string>)
            {
                List<string> list = ((List<string>)objValue);
                moreInfo.StringList = sPreEnd + string.Join(sCenter, list) + sPreEnd;
                return;
            }

        }
    }
}
