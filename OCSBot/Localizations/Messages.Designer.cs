﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OCSBot.Localizations {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Messages {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Messages() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("OCSBot.Localizations.Messages", typeof(Messages).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 你要查詢的是下面哪一種郵件.
        /// </summary>
        internal static string BOT_CARD_PLEASE_INPUT_PACKAGE_TYPE {
            get {
                return ResourceManager.GetString("BOT_CARD_PLEASE_INPUT_PACKAGE_TYPE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 我幫你聯絡客服人員, 他們會連絡您喔~.
        /// </summary>
        internal static string BOT_CREATE_TICKET {
            get {
                return ResourceManager.GetString("BOT_CREATE_TICKET", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 你問倒我了...我還沒學到這個....
        /// </summary>
        internal static string BOT_NO_ANSWERS {
            get {
                return ResourceManager.GetString("BOT_NO_ANSWERS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to  請輸入{0}號碼.
        /// </summary>
        internal static string BOT_PLEASE_INPUT_PACKAGE_NUMNER {
            get {
                return ResourceManager.GetString("BOT_PLEASE_INPUT_PACKAGE_NUMNER", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 您所查詢的{0} - {1}，目前狀態是：{2}.
        /// </summary>
        internal static string BOT_REPLY_STATUS {
            get {
                return ResourceManager.GetString("BOT_REPLY_STATUS", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 已遞送.
        /// </summary>
        internal static string STATUS_DELIVERED {
            get {
                return ResourceManager.GetString("STATUS_DELIVERED", resourceCulture);
            }
        }
    }
}
