using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Remote;
using System.Configuration;
using OpenQA.Selenium.Appium.Enums;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using OpenQA.Selenium.Support.UI;
using MobileUISpecTestFramework.Utilities;

namespace MobileUISpecTestFramework
{
    public static class DeviceInitialiser
    {
        #region Declarations
        public static AppiumDriver<IWebElement> _appiumDriver;
        public static DesiredCapabilities capabilities;
        private static string AppiumServerUri;
        private static string PlatformName;
        private static string PlatformVersion;
        private static string App_Path;
        private static string DeviceName;
        private static string AppPackage;
        private static string AppActivity;
        private static double CommandTimeout;
        private static double NewCommandTimeout;
        private static string AppiumVersion;
        private static string BrowserName;
        private static bool AutoWebView;
        private static string abdShellPath;
        #endregion

        #region LaunchApplication
        public static AppiumDriver<IWebElement> LaunchApplication()
        {
            Setconfiguration();
            capabilities = new DesiredCapabilities();
            capabilities.SetCapability(MobileCapabilityType.PlatformName, PlatformName);
            capabilities.SetCapability(MobileCapabilityType.PlatformVersion, PlatformVersion);
            capabilities.SetCapability(MobileCapabilityType.App, App_Path);
            capabilities.SetCapability(MobileCapabilityType.DeviceName, DeviceName);
            //capabilities.SetCapability(MobileCapabilityType.AppPackage, AppPackage);
            //capabilities.SetCapability(MobileCapabilityType.AppActivity, AppActivity);
            //capabilities.SetCapability(MobileCapabilityType.AppActivity,"io.appium.unlock/.Unlock");
            capabilities.SetCapability(MobileCapabilityType.NewCommandTimeout, NewCommandTimeout);
            //capabilities.SetCapability(MobileCapabilityType.AppActivity, DeviceState);

            if (PlatformName == "Android") _appiumDriver = new AndroidDriver<IWebElement>(new Uri(AppiumServerUri), capabilities, TimeSpan.FromSeconds(CommandTimeout));
            else if (PlatformName == "iOS") _appiumDriver = new IOSDriver<IWebElement>(new Uri(AppiumServerUri), capabilities, TimeSpan.FromSeconds(CommandTimeout));

            _appiumDriver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 30));

            return _appiumDriver;
        }
        #endregion

        #region Setup the Capabilities

        /// <summary>
        /// Sets the capabilities from the App configuration file
        /// </summary>
        public static void Setconfiguration()
        {
            // Setting standard configuration from App.config
            AppiumServerUri = ConfigurationManager.AppSettings["AppiumServerUri"] == null ?
            "http://127.0.0.1:4723/wd/hub" : ConfigurationManager.AppSettings["AppiumServerUri"].ToString();

            PlatformName = ConfigurationManager.AppSettings["PlatformName"] == null ?
            "Android" : ConfigurationManager.AppSettings["PlatformName"].ToString();

            PlatformVersion = ConfigurationManager.AppSettings["PlatformVersion"] == null ?
            "5.0.2" : ConfigurationManager.AppSettings["PlatformVersion"].ToString();

            App_Path = ConfigurationManager.AppSettings["App_Path"] == null ?
            Environment.CurrentDirectory + @"\App-1.2.0.89-qa-release.apk" : ConfigurationManager.AppSettings["App_Path"].ToString();

            DeviceName = ConfigurationManager.AppSettings["DeviceName"] == null ?
            String.Empty : ConfigurationManager.AppSettings["DeviceName"].ToString();

            AppPackage = ConfigurationManager.AppSettings["AppPackage"] == null ?
            String.Empty : ConfigurationManager.AppSettings["AppPackage"].ToString();

            AppActivity = ConfigurationManager.AppSettings["AppActivity"] == null ?
            String.Empty : ConfigurationManager.AppSettings["AppActivity"].ToString();

            CommandTimeout = ConfigurationManager.AppSettings["CommandTimeout"] == null ?
            180 : Convert.ToDouble(ConfigurationManager.AppSettings["CommandTimeout"].ToString());

            NewCommandTimeout = ConfigurationManager.AppSettings["NewCommandTimeout"] == null ?
            300 : Convert.ToDouble(ConfigurationManager.AppSettings["NewCommandTimeout"].ToString());

            // Additional configuration (when required)
            AppiumVersion = ConfigurationManager.AppSettings["AppiumVersion"] == null ?
            String.Empty : ConfigurationManager.AppSettings["AppiumVersion"].ToString();

            BrowserName = ConfigurationManager.AppSettings["BrowserName"] == null ?
            String.Empty : ConfigurationManager.AppSettings["BrowserName"].ToString(); //Leave empty otherwise you test on browsers

            AutoWebView = ConfigurationManager.AppSettings["AutoWebView"] == null ?
            true : Convert.ToBoolean(ConfigurationManager.AppSettings["AutoWebView"].ToString());
        }
        #endregion

        #region Manage Appium 

        /// <summary>
        /// Start Appium as a background process.
        /// </summary>
        public static bool StartAppium()
        {
            try
            {
                foreach (Process clsProcess in Process.GetProcesses())
                {
                    if (clsProcess.ProcessName.StartsWith("node"))
                    {
                        clsProcess.Kill();
                    }
                }
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "C:/Program Files (x86)/Appium/node.exe";
                startInfo.Arguments = @"""C:/Program Files (x86)/Appium/node_modules/appium/bin/appium.js"" --address " + ConfigurationManager.AppSettings["AppiumServerIP"] + " --port " + ConfigurationManager.AppSettings["AppiumServerPort"] + " --automation-name Appium --log-no-color";
                process.StartInfo = startInfo;
                process.Start();
                System.Threading.Thread.Sleep(5000);
                return true;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Utilities.LogLevel.ERROR, "Failed to start Appium server: " + ex);
                return false;
            }
        }

        /// <summary>
        /// Kills the Appium server instance
        /// </summary>
        /// <returns></returns>
        public static bool KillAppium()
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.StartsWith("node"))
                {
                    clsProcess.Kill();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        public static void CloseApp()
        {
            if (_appiumDriver != null)
            {
                _appiumDriver.CloseApp();
            }
        }

        public static void Quit()
        {
            if (_appiumDriver != null)
            {
                _appiumDriver.Dispose();
                _appiumDriver.Quit();
                _appiumDriver = null;
            }
        }

        #endregion

        #region Manage Device States
        /// <summary>
        /// Validates if the device is locked and unlocks it
        /// </summary>
        /// <returns></returns>
        public static Boolean UnLockDevice()
        {
            try
            {
                PlatformName = ConfigurationManager.AppSettings["PlatformName"] == null ? "Android" : ConfigurationManager.AppSettings["PlatformName"].ToString();

                if (PlatformName.Equals("Android"))
                {
                    abdShellPath = ConfigurationManager.AppSettings["abdShellPath"] == null ? String.Empty : ConfigurationManager.AppSettings["abdShellPath"].ToString();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo(abdShellPath, "shell am start -n io.appium.unlock/.Unlock");
                    processStartInfo.RedirectStandardInput = true;
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    processStartInfo.UseShellExecute = false;
                    System.Diagnostics.Process unlockDevice;
                    unlockDevice = System.Diagnostics.Process.Start(processStartInfo);
                    return true;
                }
                else if (PlatformName.Equals("iOS"))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.WriteLog(Utilities.LogLevel.ERROR, "Unable to Unlock the Device : " + ex);
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void HideKeyboard()
        {
            Regex InputShown = new Regex("mInputShown=?(true)|............false");
            Match match = InputShown.Match("mInputShown=true");
            string output = string.Empty;
            abdShellPath = ConfigurationManager.AppSettings["abdShellPath"] == null ? String.Empty : ConfigurationManager.AppSettings["abdShellPath"].ToString();
            ProcessStartInfo processStartInfo = new ProcessStartInfo(abdShellPath, "shell dumpsys input_method");
            processStartInfo.RedirectStandardInput = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            processStartInfo.UseShellExecute = false;

            System.Diagnostics.Process keyboard;
            keyboard = System.Diagnostics.Process.Start(processStartInfo);
            int pid = keyboard.Id;

            System.IO.StreamReader myoutput = keyboard.StandardOutput;

            output = myoutput.ReadToEnd();
            if (output.Contains("mInputShown=true"))
            {
                _appiumDriver.HideKeyboard();
            }
        }


        #endregion


    }
}
