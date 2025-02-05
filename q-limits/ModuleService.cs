﻿using q_limits.Modules;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace q_limits
{
    public static class ModuleService
    {
        public static List<IModule> KnownModules;
        public static List<Credential> KnownSuccessfulCredentials;
        
        static ModuleService()
        {
            KnownModules = new();
            KnownSuccessfulCredentials = new();

            KnownModules.Add(new HttpProxyModule());
            KnownModules.Add(new Sha256HashModule());
            KnownModules.Add(new MD5HashModule());
            KnownModules.Add(new SSHModule());
            KnownModules.Add(new HttpGetModule());
            KnownModules.Add(new HttpGetFormModule());
            KnownModules.Add(new FTPModule());
        }

        public static void FindAssessLoadModule(ProgressContext progCtx, CommandLineOptions options)
        {
            // Extract and calculate possibilities
            var credGenTask = progCtx.AddTask("[gray][[Service]][/] Generating credentials", true, 10);

            CredentialContext credContext = new();
            List<string> usernames = new();
            List<string> passwords = new();
            
            if (options.Login != null)
            {
                usernames.Add(options.Login);
            }
            if (options.LoginFile != null)
            {
                if (File.Exists(options.LoginFile))
                {
                    usernames.AddRange(File.ReadAllLines(options.LoginFile));
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{options.LoginFile}' was not found[/]");
                }
            }
            if (options.Password != null)
            {
                passwords.Add(options.Password);
            }
            if (options.PasswordFile != null)
            {
                if (File.Exists(options.PasswordFile))
                {
                    string[] flCont = File.ReadAllLines(options.PasswordFile);
                    passwords.AddRange(flCont);
                }
                else
                {
                    AnsiConsole.MarkupLine($"[red]File '{options.PasswordFile}' was not found[/]");
                }
            }
            /*if (false)
            {
                string[] splt = argD["x"].Split(":");
                if (splt.Length == 3)
                {
                    try
                    {
                        int min = int.Parse(splt[0]);
                        int max = int.Parse(splt[1]);
                        bool lowercase = splt[2].Contains("a");
                        bool uppercase = splt[2].Contains("A");
                        bool numbers = splt[2].Contains("1");
                        bool symbols = splt[2].Contains("!");

                        string possiblechars = "";
                        if (lowercase)
                        {
                            possiblechars += "abcdefghijklmnopqrstuvwxyz";
                        }
                        if (uppercase)
                        {
                            possiblechars += "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                        }
                        if (numbers)
                        {
                            possiblechars += "0123456789";
                        }
                        if (symbols)
                        {
                            possiblechars += "!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?~`";
                        }
                        
                        // TODO: Generate every possible combination with a progressbar and add it to 'passwords'
                    }
                    catch (Exception)
                    {
                        AnsiConsole.MarkupLine("[red]Generation (x) parameter failed[/]");
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Generation (x) parameter requires 3 chunks such as: '1:3:aA1!'[/]");
                }
            }*/ // TODO: Finish dive algorithm

            credContext.Usernames = usernames.ToArray();
            credContext.Passwords = passwords.ToArray();

            credGenTask.Value = credGenTask.MaxValue;

            // Find correct module
            var findModTask = progCtx.AddTask("[gray][[Service]][/] Finding module", true, 10);

            bool func(IModule x) => x.ID.ToLower() == options.Module.ToLower();

            if (!KnownModules.Any(func)) // Ignore case
            {
                AnsiConsole.MarkupLine($"[red]Unknown module '{options.Module.ToLower()}'[/]");
                return;
            }

            IModule loadingModule = KnownModules.First(func);

            findModTask.Value = findModTask.MaxValue;
            loadingModule.Load(options, credContext, progCtx);
        }

        public static void ReportSuccess(string dest, Credential cred, string loginName = "login", string passName = "password")
        {
            KnownSuccessfulCredentials.Add(cred);
            AnsiConsole.MarkupLine($"[[[blue underline]{DateTime.Now}[/]]] Credentials retrieved for [blue]{dest}[/] > {loginName}: {(cred.Key != null ? $"[green]{cred.Key}[/]" : "[red]NULL[/]")}  {passName}: {(cred.Value != null ? $"[green]{cred.Value}[/]" : "[red]NULL[/]")}");
        }
    }
}
