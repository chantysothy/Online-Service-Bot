﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AgentBot.Localizations {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AgentBot.Localizations.Messages", typeof(Messages).Assembly);
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
        ///   Looks up a localized string similar to 目前沒有任何與客戶的直接對話.
        /// </summary>
        internal static string BOT_CASE_CLOSED {
            get {
                return ResourceManager.GetString("BOT_CASE_CLOSED", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 您好，請輸入help取得命令列表.
        /// </summary>
        internal static string BOT_GET_HELP {
            get {
                return ResourceManager.GetString("BOT_GET_HELP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 命令：&lt;br/&gt;&lt;br/&gt;登入：login&lt;br/&gt;&lt;br/&gt;登出：logout&lt;br/&gt;&lt;br/&gt;回覆：reply:{回答}.
        /// </summary>
        internal static string BOT_HELP {
            get {
                return ResourceManager.GetString("BOT_HELP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 您已經登出工作站!.
        /// </summary>
        internal static string BOT_LOGGOUT {
            get {
                return ResourceManager.GetString("BOT_LOGGOUT", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請登入工作站.
        /// </summary>
        internal static string BOT_LOGIN_CARD_TITLE {
            get {
                return ResourceManager.GetString("BOT_LOGIN_CARD_TITLE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 請登入.
        /// </summary>
        internal static string BOT_PLEASE_LOGIN {
            get {
                return ResourceManager.GetString("BOT_PLEASE_LOGIN", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to .
        /// </summary>
        internal static string String1 {
            get {
                return ResourceManager.GetString("String1", resourceCulture);
            }
        }
    }
}
