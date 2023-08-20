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
     *    2023/08/13 BreezeeHui 键设置增加默认值、不加引号。
     *    2023/08/18 BreezeeHui 字符比较忽略大小写（Equals(sOne, StringComparison.OrdinalIgnoreCase) 代替 Equals(sOne)）。子配置支持支持-&,;，；分隔
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
        /// <summary>
        /// 默认值：示例：D-默认值-R-N
        /// </summary>
        public string DefaultValue = string.Empty;
        /// <summary>
        /// 是否默认值不加引号：默认都加上。不要时可设置为ture
        /// </summary>
        public bool IsDefaultValueNoQuotationMark = false;
        /// <summary>
        /// 是否默认值必须值替换：默认为false，即当值来使用，使用参数化。为ture时，是直接使用值，如函数等
        /// </summary>
        public bool IsDefaultValueValueReplace = false;
        /// <summary>
        /// 值不加引号：默认都加上。不要时可设置为ture
        /// </summary>
        public bool IsNoQuotationMark = false;
        /**
         * 构建【键更多信息】对象
         * @param sKeyMore 键更多信息字符，例如：CITY_NAME:N
         * @return
         */
        public static KeyMoreInfo build(string sKeyMore, object objValue)
        {
            KeyMoreInfo moreInfo = new KeyMoreInfo();
            //配置大类分隔
            string[] arr = sKeyMore.Split(StaticConstants.keyBigTypeSpit);//也支持中文冒号
            for (int i = 0; i < arr.Length; i++)
            {
                if (i == 0) continue;
                string sOne = arr[i].trim();
                if (string.IsNullOrEmpty(sOne)) continue;
                //配置小类分隔
                string[] sMoreArr = sOne.Split(StaticConstants.keySmallTypeSpit);
                sOne = sMoreArr[0].trim();

                if (SqlKeyConfig.V_MUST.Equals(sOne, StringComparison.OrdinalIgnoreCase))
                {
                    moreInfo.Nullable = false;//是否可空
                }
                else if (SqlKeyConfig.V_REPLACE.Equals(sOne, StringComparison.OrdinalIgnoreCase))
                {
                    moreInfo.MustValueReplace = true;//必须替换
                }
                else if (SqlKeyConfig.CFG_FIRST.Equals(sOne,StringComparison.OrdinalIgnoreCase))
                {
                    moreInfo.IsFirst = true;//是否优先使用本配置
                }
                else if (SqlKeyConfig.STRING_LIST.Equals(sOne, StringComparison.OrdinalIgnoreCase))  //字符列表
                {
                    listConvert(objValue, moreInfo, true);
                }
                else if (SqlKeyConfig.INTEGE_LIST.Equals(sOne, StringComparison.OrdinalIgnoreCase)) //整型列表
                {
                    listConvert(objValue, moreInfo, false);
                }
                else if (SqlKeyConfig.V_DEFAULT.Equals(sOne, StringComparison.OrdinalIgnoreCase)) //默认值
                {
                    for (int j = 1; j < sMoreArr.Length; j++)
                    {
                        if (j == 1)
                        {
                            moreInfo.DefaultValue = sMoreArr[1].trim();
                        }
                        else
                        {
                            if (SqlKeyConfig.V_REPLACE.Equals(sMoreArr[j].trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                moreInfo.IsDefaultValueValueReplace = true;//默认值必须值替换
                            }
                            if (SqlKeyConfig.V_NO_QUOTATION_MARK.Equals(sMoreArr[j].trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                moreInfo.IsDefaultValueNoQuotationMark = true;//默认值不加引号
                            }
                        }
                    }
                }
                else if (SqlKeyConfig.V_NO_QUOTATION_MARK.Equals(sOne, StringComparison.OrdinalIgnoreCase)) //值不加引号
                {
                    moreInfo.IsNoQuotationMark = true;
                }
                else
                {
                    //throw new Exception("暂不支持的配置项！"+sOne);
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
