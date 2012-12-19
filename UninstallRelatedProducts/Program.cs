﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CommandLine.Text;

namespace UninstallRelatedProducts
{
    class Program
    {
        class Options : CommandLineOptionsBase
        {
            [Option(null, "upgradecode", Required = true, HelpText = "Upgrade code of the products to uninstall.")]
            public string UpgradeCode { get; set; }

            [Option(null, "maxversion",
                HelpText = "Maximum product version to uninstall. " +
                    "Products with a version greater than this number will be skipped. " +
                    "If not specified, all related products will be uninstalled.")]
            public string MaxVersion { get; set; }

            [Option(null, "quiet", DefaultValue = false, HelpText = "Quiet mode, no user interaction")]
            public bool Silent { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this, c => HelpText.DefaultParsingErrorsHandler(this, c));
            }
        }

        static void Main(string[] args)
        {
            try
            {
                var options = new Options();
                if (!CommandLineParser.Default.ParseArguments(args, options))
                {
                    // Failed to parse arguments
                    // Arguments help text is already printed to stdout
                    Environment.Exit(-1);
                }

                // Additional arguments parsing
                var upgradeCode = Guid.Parse(options.UpgradeCode);
                var maxVersion = default(Version);
                if (!string.IsNullOrEmpty(options.MaxVersion))
                    maxVersion = Version.Parse(options.MaxVersion);

                var productCodes = Msi.GetRelatedProducts(upgradeCode).ToList();
                Console.WriteLine("Number of related products found: " + productCodes.Count);
                foreach (var product in productCodes)
                {
                    Console.WriteLine("Product code: " + product);

                    var version = Msi.GetVersion(product);
                    Console.WriteLine("Product version: " + version);

                    if (maxVersion == null || version <= maxVersion)
                    {
                        Console.WriteLine("Uninstalling.");
                        Msi.Uninstall(product, options.Silent);
                    }
                    else
                    {
                        Console.WriteLine("Skipping.");
                    }
                }
            }
            catch (Exception e)
            {
                // TODO how to write errors to burn log files?
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
