﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Collections;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.Mvc.Html;
using Todoku.Models;

namespace Todoku.Models
{
    public static class Method
    {
        public static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static Object GetListObject(Type type, Object obj)
        {
            var temp = Activator.CreateInstance(Type.GetType(type.FullName), null);
            Dictionary<String, Object> dict = (Dictionary<String, Object>)obj;
            foreach (KeyValuePair<String, Object> entry in dict)
            {
                PropertyInfo prop = type.GetProperty(entry.Key);
                if (entry.Value == null)
                {
                    prop.SetValue(temp, entry.Value, null);
                }
                else if (!entry.Value.ToString().Contains("System") && prop.CanWrite)
                {
                    switch (prop.PropertyType.Name)
                    {
                        case "Int32":
                            prop.SetValue(temp, Convert.ToInt32(entry.Value), null);
                            break;
                        case "Decimal":
                            prop.SetValue(temp, Convert.ToDecimal(entry.Value), null);
                            break;
                        case "Boolean":
                            prop.SetValue(temp, Convert.ToBoolean(entry.Value), null);
                            break;
                        default:
                            prop.SetValue(temp, entry.Value, null);
                            break;
                    }
                }
                else if(entry.Value.ToString() != "" && entry.Value != null)
                {
                    String ClassName = "";
                    switch (entry.Key) 
                    {
                        case "detail":
                            ClassName = "ProductsDetails";
                            break;
                        default :
                            ClassName = UppercaseFirst(entry.Key);
                            break;
                    }
                    Type tempType = Type.GetType("Todoku.Models." + ClassName);
                    var tempObj = GetListObject(tempType, entry.Value);
                    prop.SetValue(temp, tempObj, null);
                }
            }
            return temp;
        }

        public static List<T> ConvertJsonObject<T>(Object[] list)
        {

            List<T> listTemp = new List<T>();
            foreach (var obj in list)
            {
                Type t = typeof(T);
                listTemp.Add((T)GetListObject(t, obj));
            }
            return listTemp;
        }

        public static String GetTransactionCode(BusinessLayer db, String Code) 
        {
            Int32 Count = 0;
            String Format = String.Format("{0}/{1}", Code, DateTime.Now.ToString("yyyyMMdd"));
            switch(Code)
            {
                case SystemSetting.AgentCode : 
                    break;
                case SystemSetting.MemberCode : 
                    break;
                case SystemSetting.MerchantCode : 
                    break;
                case SystemSetting.RegisMerchantCode :
                    Count = db.merchantRegistrations.Where(x => x.RegistrationCode.Contains(Format)).Count() + 1;
                    break;
                case SystemSetting.RegisAgenCode : 
                    break;
            }
            
            return Format + "/" + Count.ToString("0000");
        }

        public static bool UploadFileValidation(HttpPostedFileBase file, Int32? MaxSize, String[] extension)
        {
            if (file.ContentLength <= 0) return false;
            if (!extension.Contains(Path.GetExtension(file.FileName))) return false;
            if (MaxSize != null && file.ContentLength > MaxSize) return false;
            return true;
        }

        public static bool UploadFileValidation(HttpPostedFileBase file, String[] extension)
        {
            return UploadFileValidation(file, null, extension);
        }
    }

    public class DecimalModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            return valueProviderResult == null ? base.BindModel(controllerContext, bindingContext) : Convert.ToDecimal(valueProviderResult.AttemptedValue);
            // of course replace with your custom conversion logic
        }
    }
}

namespace System.Web.Mvc 
{
    public static class HtmlButtonExtension
    {
        public static MvcHtmlString Button(this HtmlHelper helper,
                                           string innerHtml,
                                           object htmlAttributes)
        {
            return Button(helper, innerHtml, HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes)
            );
        }

        public static MvcHtmlString Button(this HtmlHelper helper,
                                           string innerHtml,
                                           IDictionary<string, object> htmlAttributes)
        {
            //var builder = new TagBuilder("button");
            //builder.InnerHtml = innerHtml;
            var builder = new TagBuilder("input");
            builder.MergeAttribute("type", "button");
            builder.MergeAttribute("value", innerHtml);
            builder.MergeAttributes(htmlAttributes);
            return MvcHtmlString.Create(builder.ToString());
        }

        public static MvcHtmlString DisplayWithBreaksFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
            var model = html.Encode(metadata.Model).Replace("\r\n", "<br />\r\n");
            model = html.Encode(metadata.Model).Replace("\n", "<br />");
            if (String.IsNullOrEmpty(model))
                return MvcHtmlString.Empty;

            return MvcHtmlString.Create(model);
        }
    }

    public static class HtmlMenu
    {
        public static MvcHtmlString Menu(this HtmlHelper html, List<Todoku.Models.Menu> menus, Object htmlAttributes)
        {
            
            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                if (htmlAttributes.GetType().GetProperty("id") != null)
                {
                    String txtClass = htmlAttributes.GetType().GetProperty("id").GetValue(htmlAttributes,null).ToString();
                    writer.AddAttribute("id", txtClass);
                }
                if (htmlAttributes.GetType().GetProperty("class") != null)
                {
                    String txtClass = htmlAttributes.GetType().GetProperty("class").GetValue(htmlAttributes, null).ToString();
                    writer.AddAttribute("class", txtClass);
                }
                if (htmlAttributes.GetType().GetProperty("style") != null) 
                {
                    String temp = htmlAttributes.GetType().GetProperty("style").GetValue(htmlAttributes, null).ToString();
                    writer.AddAttribute("style", temp);
                }
                writer.RenderBeginTag("ul");
                foreach (Todoku.Models.Menu obj in menus.Where(x => x.ParentID == null))
                {
                    writer.Write(MenuItem(menus, obj, 1));
                }
                writer.RenderEndTag();//ul
            }
            return new MvcHtmlString(stringWriter.ToString());
        }

        public static String MenuItem(List<Todoku.Models.Menu> menus, Todoku.Models.Menu menu, Int32 Level) 
        {
            StringWriter stringWriter = new StringWriter();

            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                List<Todoku.Models.Menu> childs = menus.Where(x => x.ParentID == menu.MenuID).ToList();
                if (childs.Count() > 0)
                {
                    if (menu.parent == null)
                    {
                        writer.AddAttribute("class", "ui-widget-header");
                        writer.RenderBeginTag(HtmlTextWriterTag.Li);
                        writer.Write(menu.MenuName);
                        writer.RenderEndTag();//li
                        Int32 temp = ++Level;
                        foreach (Todoku.Models.Menu child in childs)
                        {
                            writer.Write(MenuItem(menus, child, temp));
                        }
                    }
                    else 
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Li);
                        writer.Write(menu.MenuName);
                        writer.RenderBeginTag(HtmlTextWriterTag.Ul);
                        Int32 temp = ++Level;
                        foreach (Todoku.Models.Menu child in menu.childs)
                        {
                            writer.Write(MenuItem(menus, child, temp));
                        }
                        writer.RenderEndTag();//ul
                        writer.RenderEndTag();//li
                    }
                }
                else
                {
                    if(Level > 2) writer.AddAttribute("style", "width: 150px");
                    writer.RenderBeginTag(HtmlTextWriterTag.Li);
                    writer.AddAttribute("href", String.Format("{0}{1}", SystemSetting.Default_URL, menu.Path));
                    
                    writer.RenderBeginTag(HtmlTextWriterTag.A);
                    writer.Write(menu.MenuName);
                    writer.RenderEndTag();//a
                    writer.RenderEndTag();//li
                }
                return stringWriter.ToString();
            }
        }
    }
}