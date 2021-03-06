﻿// Author: Ryan Cobb (@cobbr_io)
// Project: Covenant (https://github.com/cobbr/Covenant)
// License: GNU GPLv3

using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using Covenant.Models;
using Covenant.Models.Grunts;
using Covenant.Models.Launchers;
using Covenant.Models.Listeners;

namespace Covenant.Controllers
{
	[Authorize]
	[ApiController]
	[Route("api/launchers")]
    public class LauncherApiController : Controller
    {
        private readonly CovenantContext _context;

        public LauncherApiController(CovenantContext context)
        {
            _context = context;
        }

        // GET: api/launchers
        // <summary>
        // Get PowerShellLauncher
        // </summary>
        [HttpGet(Name = "GetLaunchers")]
        public ActionResult<IEnumerable<Launcher>> Get()
        {
            return _context.Launchers.ToList();
        }

        // GET api/launchers/binary
        // <summary>
        // Get BinaryLauncher
        // </summary>
        [HttpGet("binary", Name = "GetBinaryLauncher")]
        public ActionResult<BinaryLauncher> GetBinaryLauncher()
        {
            BinaryLauncher launcher = (BinaryLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Binary);
            if (launcher == null)
            {
                return NotFound($"NotFound - BinaryLauncher");
            }
            return launcher;
        }

        // POST api/stagers/launcher
        // <summary>
        // Generate BinaryLauncher LauncherString
        // </summary>
        [HttpPost("binary", Name = "GenerateBinaryLauncher")]
        public ActionResult<BinaryLauncher> GenerateBinaryLauncher()
        {
            BinaryLauncher launcher = (BinaryLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Binary);
            if (launcher == null)
            {
                return NotFound($"NotFound - BinaryLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Launcher with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }
            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();

            launcher.GetLauncher(listener, grunt, profile);

            _context.Launchers.Update(launcher);
            _context.SaveChanges();
            return launcher;
        }

        // POST api/launchers/binary/hosted
        // <summary>
        // Generate a BinaryLauncher that points to a hosted binary file
        // </summary>
        [HttpPost("binary/hosted", Name = "GenerateBinaryHostedFileLauncher")]
        public ActionResult<BinaryLauncher> GenerateBinaryHostedFileLauncher(HostedFile hostedFile)
        {
            BinaryLauncher launcher = (BinaryLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Binary);
            if (launcher == null)
            {
                return NotFound($"NotFound - BinaryLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/powershell
        // <summary>
        // Edit BinaryLauncher
        // </summary>
        [HttpPut("binary", Name = "PutBinaryLauncher")]
        public ActionResult<BinaryLauncher> PutBinaryLauncher([FromBody]BinaryLauncher binaryLauncher)
        {
            BinaryLauncher launcher = (BinaryLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Binary);
            if (launcher == null || launcher.Id != binaryLauncher.Id)
            {
                return NotFound($"NotFound - BinaryLauncher with id: {binaryLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == binaryLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = binaryLauncher.ListenerId;
            }
            launcher.CommType = binaryLauncher.CommType;
            launcher.SMBPipeName = binaryLauncher.SMBPipeName;
            launcher.ValidateCert = binaryLauncher.ValidateCert;
            launcher.UseCertPinning = binaryLauncher.UseCertPinning;
            launcher.Delay = binaryLauncher.Delay;
            launcher.JitterPercent = binaryLauncher.JitterPercent;
            launcher.ConnectAttempts = binaryLauncher.ConnectAttempts;
            launcher.KillDate = binaryLauncher.KillDate;
            launcher.DotNetFrameworkVersion = binaryLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = binaryLauncher.LauncherString;
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/powershell
        // <summary>
        // Get PowerShellLauncher
        // </summary>
        [HttpGet("powershell", Name = "GetPowerShellLauncher")]
        public ActionResult<PowerShellLauncher> GetPowerShellLauncher()
        {
            PowerShellLauncher launcher = (PowerShellLauncher) _context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.PowerShell);
            if (launcher == null)
            {
                return NotFound($"NotFound - PowerShellLauncher");
            }
            return launcher;
        }

        // POST api/launchers/powershell
        // <summary>
        // Generate PowerShellLauncher LauncherString
        // </summary>
        [HttpPost("powershell", Name = "GeneratePowerShellLauncher")]
        public ActionResult<PowerShellLauncher> GeneratePowerShellLauncher()
        {
            PowerShellLauncher launcher = (PowerShellLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.PowerShell);
            if (launcher == null)
            {
                return NotFound($"NotFound - PowerShellLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/powershell/hosted
        // <summary>
        // Generate a PowerShellLauncher that points to a hosted powershell file
        // </summary>
        [HttpPost("powershell/hosted", Name = "GeneratePowerShellHostedFileLauncher")]
        public ActionResult<PowerShellLauncher> GeneratePowerShellHostedFileLauncher(HostedFile hostedFile)
        {
            PowerShellLauncher launcher = (PowerShellLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.PowerShell);
            if (launcher == null)
            {
                return NotFound($"NotFound - PowerShellLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/powershell
        // <summary>
        // Edit PowerShellLauncher
        // </summary>
        [HttpPut("powershell", Name = "PutPowerShellLauncher")]
        public ActionResult<PowerShellLauncher> PutPowerShellLauncher([FromBody] PowerShellLauncher powerShellLauncher)
        {
            PowerShellLauncher launcher = (PowerShellLauncher) _context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.PowerShell);
            if (launcher == null || launcher.Id != powerShellLauncher.Id)
            {
                return NotFound($"NotFound - PowerShellLauncher with id: {powerShellLauncher.Id}");
            }

            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == powerShellLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = powerShellLauncher.ListenerId;
            }

            launcher.CommType = powerShellLauncher.CommType;
            launcher.SMBPipeName = powerShellLauncher.SMBPipeName;
            launcher.ValidateCert = powerShellLauncher.ValidateCert;
            launcher.UseCertPinning = powerShellLauncher.UseCertPinning;
            launcher.Delay = powerShellLauncher.Delay;
            launcher.JitterPercent = powerShellLauncher.JitterPercent;
            launcher.ConnectAttempts = powerShellLauncher.ConnectAttempts;
            launcher.KillDate = powerShellLauncher.KillDate;
            launcher.ParameterString = powerShellLauncher.ParameterString;
            launcher.DotNetFrameworkVersion = powerShellLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = powerShellLauncher.LauncherString;
            _context.Launchers.Update(launcher);

            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/msbuild
        // <summary>
        // Get MSBuildLauncher
        // </summary>
        [HttpGet("msbuild", Name = "GetMSBuildLauncher")]
        public ActionResult<MSBuildLauncher> GetMSBuildLauncher()
        {
            MSBuildLauncher launcher = (MSBuildLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.MSBuild);
            if (launcher == null)
            {
                return NotFound($"NotFound - MSBuildLauncher");
            }
            return launcher;
        }

        // POST api/launchers/msbuild
        // <summary>
        // Generate MSBuild LauncherString
        // </summary>
        [HttpPost("msbuild", Name = "GenerateMSBuildLauncher")]
        public ActionResult<MSBuildLauncher> GenerateMSBuildLauncher()
        {
            MSBuildLauncher launcher = (MSBuildLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.MSBuild);
            if (launcher == null)
            {
                return NotFound($"NotFound - MSBuildLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/msbuild/hosted
        // <summary>
        // Generate a MSBuildLauncher that points to a hosted msbuild xml file
        // </summary>
        [HttpPost("msbuild/hosted", Name = "GenerateMSBuildHostedFileLauncher")]
        public ActionResult<MSBuildLauncher> GenerateMSBuildHostedFileLauncher(HostedFile hostedFile)
        {
            MSBuildLauncher launcher = (MSBuildLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.MSBuild);
            if (launcher == null)
            {
                return NotFound($"NotFound - MSBuildLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/msbuild
        // <summary>
        // Edit MSBuildLauncher
        // </summary>
        [HttpPut("msbuild", Name = "PutMSBuildLauncher")]
        public ActionResult<MSBuildLauncher> PutMSBuildLauncher([FromBody] MSBuildLauncher msbuildLauncher)
        {
            MSBuildLauncher launcher = (MSBuildLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.MSBuild);
            if (launcher == null || launcher.Id != msbuildLauncher.Id)
            {
                return NotFound($"NotFound - MSBuildLauncher wiht id: {msbuildLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == msbuildLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = msbuildLauncher.ListenerId;
            }
            launcher.CommType = msbuildLauncher.CommType;
            launcher.SMBPipeName = msbuildLauncher.SMBPipeName;
            launcher.ValidateCert = msbuildLauncher.ValidateCert;
            launcher.UseCertPinning = msbuildLauncher.UseCertPinning;
            launcher.Delay = msbuildLauncher.Delay;
            launcher.JitterPercent = msbuildLauncher.JitterPercent;
            launcher.ConnectAttempts = msbuildLauncher.ConnectAttempts;
            launcher.KillDate = msbuildLauncher.KillDate;
            launcher.DotNetFrameworkVersion = msbuildLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = msbuildLauncher.LauncherString;
            launcher.DiskCode = msbuildLauncher.DiskCode;
            launcher.StagerCode = msbuildLauncher.StagerCode;
            launcher.TargetName = msbuildLauncher.TargetName;
            launcher.TaskName = msbuildLauncher.TaskName;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/installutil
        // <summary>
        // Get InstallUtilLauncher
        // </summary>
        [HttpGet("installutil", Name = "GetInstallUtilLauncher")]
        public ActionResult<InstallUtilLauncher> GetInstallUtilLauncher()
        {
            InstallUtilLauncher launcher = (InstallUtilLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.InstallUtil);
            if (launcher == null)
            {
                return NotFound($"NotFound - InstallUtilLauncher");
            }
            return launcher;
        }

        // POST api/launchers/installutil
        // <summary>
        // Generate InstallUtil LauncherString
        // </summary>
        [HttpPost("installutil", Name = "GenerateInstallUtilLauncher")]
        public ActionResult<InstallUtilLauncher> GenerateInstallUtilLauncher()
        {
            InstallUtilLauncher launcher = (InstallUtilLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.InstallUtil);
            if (launcher == null)
            {
                return NotFound($"NotFound - InstallUtilLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/installutil/hosted
        // <summary>
        // Generate a InstallUtilLauncher that points to a hosted msbuild xml file
        // </summary>
        [HttpPost("installutil/hosted", Name = "GenerateInstallUtilHostedFileLauncher")]
        public ActionResult<InstallUtilLauncher> GenerateInstallUtilHostedFileLauncher(HostedFile hostedFile)
        {
            InstallUtilLauncher launcher = (InstallUtilLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.InstallUtil);
            if (launcher == null)
            {
                return NotFound($"NotFound - InstallUtilLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/installutil
        // <summary>
        // Edit InstallUtilLauncher
        // </summary>
        [HttpPut("installutil", Name = "PutInstallUtilLauncher")]
        public ActionResult<InstallUtilLauncher> PutInstallUtilLauncher([FromBody] InstallUtilLauncher installutilLauncher)
        {
            InstallUtilLauncher launcher = (InstallUtilLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.InstallUtil);
            if (launcher == null || launcher.Id != installutilLauncher.Id)
            {
                return NotFound($"NotFound - InstallUtilLauncher with id: {installutilLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == installutilLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = installutilLauncher.ListenerId;
            }
            launcher.CommType = installutilLauncher.CommType;
            launcher.SMBPipeName = installutilLauncher.SMBPipeName;
            launcher.ValidateCert = installutilLauncher.ValidateCert;
            launcher.UseCertPinning = installutilLauncher.UseCertPinning;
            launcher.Delay = installutilLauncher.Delay;
            launcher.JitterPercent = installutilLauncher.JitterPercent;
            launcher.ConnectAttempts = installutilLauncher.ConnectAttempts;
            launcher.KillDate = installutilLauncher.KillDate;
            launcher.DotNetFrameworkVersion = installutilLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = installutilLauncher.LauncherString;
            launcher.DiskCode = installutilLauncher.DiskCode;
            launcher.StagerCode = installutilLauncher.StagerCode;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/wmic
        // <summary>
        // Get WmicLauncher
        // </summary>
        [HttpGet("wmic", Name = "GetWmicLauncher")]
        public ActionResult<WmicLauncher> GetWmicLauncher()
        {
            WmicLauncher launcher = (WmicLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wmic);
            if (launcher == null)
            {
                return NotFound($"NotFound - WmicLauncher");
            }
            return launcher;
        }

        // POST api/launchers/wmic
        // <summary>
        // Generate WmicLauncher LauncherString
        // </summary>
        [HttpPost("wmic", Name = "GenerateWmicLauncher")]
        public ActionResult<WmicLauncher> GenerateWmicLauncher()
        {
            WmicLauncher launcher = (WmicLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wmic);
            if (launcher == null)
            {
                return NotFound($"NotFound - WmicLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/wmic/hosted
        // <summary>
        // Generate a WmicLauncher that points to a hosted xls file
        // </summary>
        [HttpPost("wmic/hosted", Name = "GenerateWmicHostedFileLauncher")]
        public ActionResult<WmicLauncher> GenerateWmicHostedFileLauncher(HostedFile hostedFile)
        {
            WmicLauncher launcher = (WmicLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wmic);
            if (launcher == null)
            {
                return NotFound($"NotFound - WmicLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/wmic
        // <summary>
        // Edit WmicLauncher
        // </summary>
        [HttpPut("wmic", Name = "PutWmicLauncher")]
        public ActionResult<WmicLauncher> PutWscriptLauncher([FromBody]WmicLauncher wmicLauncher)
        {
            WmicLauncher launcher = (WmicLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wmic);
            if (launcher == null || launcher.Id != wmicLauncher.Id)
            {
                return NotFound($"NotFound - WmicLauncher with id: {wmicLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == wmicLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = wmicLauncher.ListenerId;
            }
            launcher.CommType = wmicLauncher.CommType;
            launcher.SMBPipeName = wmicLauncher.SMBPipeName;
            launcher.ValidateCert = wmicLauncher.ValidateCert;
            launcher.UseCertPinning = wmicLauncher.UseCertPinning;
            launcher.Delay = wmicLauncher.Delay;
            launcher.JitterPercent = wmicLauncher.JitterPercent;
            launcher.ConnectAttempts = wmicLauncher.ConnectAttempts;
            launcher.KillDate = wmicLauncher.KillDate;
            launcher.ScriptLanguage = wmicLauncher.ScriptLanguage;
            launcher.DotNetFrameworkVersion = wmicLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = wmicLauncher.LauncherString;
            launcher.DiskCode = wmicLauncher.DiskCode;
            launcher.StagerCode = wmicLauncher.StagerCode;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/regsvr32
        // <summary>
        // Get Regsvr32Launcher
        // </summary>
        [HttpGet("regsvr32", Name = "GetRegsvr32Launcher")]
        public ActionResult<Regsvr32Launcher> GetRegsvr32Launcher()
        {
            Regsvr32Launcher launcher = (Regsvr32Launcher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Regsvr32);
            if (launcher == null)
            {
                return NotFound($"NotFound - Regsvr32Launcher");
            }
            return launcher;
        }

        // POST api/launcher/regsvr32
        // <summary>
        // Generate Regsvr32Launcher LauncherString
        // </summary>
        [HttpPost("regsvr32", Name = "GenerateRegsvr32Launcher")]
        public ActionResult<Regsvr32Launcher> GenerateRegsvr32Launcher()
        {
            Regsvr32Launcher launcher = (Regsvr32Launcher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Regsvr32);
            if (launcher == null)
            {
                return NotFound($"NotFound - Regsvr32Launcher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/regsvr32/hosted
        // <summary>
        // Generate a Regsvr32Launcher that points to a hosted sct file
        // </summary>
        [HttpPost("regsvr32/hosted", Name = "GenerateRegsvr32HostedFileLauncher")]
        public ActionResult<Regsvr32Launcher> GenerateRegsvr32HostedFileLauncher(HostedFile hostedFile)
        {
            Regsvr32Launcher launcher = (Regsvr32Launcher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Regsvr32);
            if (launcher == null)
            {
                return NotFound($"NotFound - Regsvr32Launcher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/regsvr32
        // <summary>
        // Edit Regsvr32Launcher
        // </summary>
        [HttpPut("regsvr32", Name = "PutRegsvr32Launcher")]
        public ActionResult<Regsvr32Launcher> PutRegsvr32Launcher([FromBody]Regsvr32Launcher regsvr32Launcher)
        {
            Regsvr32Launcher launcher = (Regsvr32Launcher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Regsvr32);
            if (launcher == null || launcher.Id != regsvr32Launcher.Id)
            {
                return NotFound($"NotFound - Regsvr32Launcher with id: {regsvr32Launcher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == regsvr32Launcher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = regsvr32Launcher.ListenerId;
            }
            launcher.CommType = regsvr32Launcher.CommType;
            launcher.SMBPipeName = regsvr32Launcher.SMBPipeName;
            launcher.ValidateCert = regsvr32Launcher.ValidateCert;
            launcher.UseCertPinning = regsvr32Launcher.UseCertPinning;
            launcher.Delay = regsvr32Launcher.Delay;
            launcher.JitterPercent = regsvr32Launcher.JitterPercent;
            launcher.ConnectAttempts = regsvr32Launcher.ConnectAttempts;
            launcher.KillDate = regsvr32Launcher.KillDate;
            launcher.ParameterString = regsvr32Launcher.ParameterString;
            launcher.DllName = regsvr32Launcher.DllName;
            launcher.ScriptLanguage = regsvr32Launcher.ScriptLanguage;
            launcher.DotNetFrameworkVersion = regsvr32Launcher.DotNetFrameworkVersion;
            launcher.LauncherString = regsvr32Launcher.LauncherString;
            launcher.DiskCode = regsvr32Launcher.DiskCode;
            launcher.StagerCode = regsvr32Launcher.StagerCode;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/mshta
        // <summary>
        // Get MshtaLauncher
        // </summary>
        [HttpGet("mshta", Name = "GetMshtaLauncher")]
        public ActionResult<MshtaLauncher> GetMshtaLauncher()
        {
            MshtaLauncher launcher = (MshtaLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Mshta);
            if (launcher == null)
            {
                return NotFound($"NotFound - Regsvr32Launcher");
            }
            return launcher;
        }

        // POST api/launchers/mshta
        // <summary>
        // Generate MshtaLauncher LauncherString
        // </summary>
        [HttpPost("mshta", Name = "GenerateMshtaLauncher")]
        public ActionResult<MshtaLauncher> GenerateMshtaLauncher()
        {
            MshtaLauncher launcher = (MshtaLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Mshta);
            if (launcher == null)
            {
                return NotFound($"NotFound - MshtaLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/mshta/hosted
        // <summary>
        // Generate a MshtaLauncher that points to a hosted sct file
        // </summary>
        [HttpPost("mshta/hosted", Name = "GenerateMshtaHostedFileLauncher")]
        public ActionResult<MshtaLauncher> GenerateMshtaHostedFileLauncher(HostedFile hostedFile)
        {
            MshtaLauncher launcher = (MshtaLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Mshta);
            if (launcher == null)
            {
                return NotFound($"NotFound - MshtaLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/mshta
        // <summary>
        // Edit MshtaLauncher
        // </summary>
        [HttpPut("mshta", Name = "PutMshtaLauncher")]
        public ActionResult<MshtaLauncher> PutMshtaLauncher([FromBody] MshtaLauncher mshtaLauncher)
        {
            MshtaLauncher launcher = (MshtaLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Mshta);
            if (launcher == null || launcher.Id != mshtaLauncher.Id)
            {
                return NotFound($"NotFound - MshtaLauncher with id: {mshtaLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == mshtaLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = mshtaLauncher.ListenerId;
            }
            launcher.CommType = mshtaLauncher.CommType;
            launcher.SMBPipeName = mshtaLauncher.SMBPipeName;
            launcher.ValidateCert = mshtaLauncher.ValidateCert;
            launcher.UseCertPinning = mshtaLauncher.UseCertPinning;
            launcher.Delay = mshtaLauncher.Delay;
            launcher.JitterPercent = mshtaLauncher.JitterPercent;
            launcher.ConnectAttempts = mshtaLauncher.ConnectAttempts;
            launcher.KillDate = mshtaLauncher.KillDate;
            launcher.ScriptLanguage = mshtaLauncher.ScriptLanguage;
            launcher.DotNetFrameworkVersion = mshtaLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = mshtaLauncher.LauncherString;
            launcher.DiskCode = mshtaLauncher.DiskCode;
            launcher.StagerCode = mshtaLauncher.StagerCode;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/cscript
        // <summary>
        // Get CscriptLauncher
        // </summary>
        [HttpGet("cscript", Name = "GetCscriptLauncher")]
        public ActionResult<CscriptLauncher> GetCscriptLauncher()
        {
            CscriptLauncher launcher = (CscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Cscript);
            if (launcher == null)
            {
                return NotFound($"NotFound - CscriptLauncher");
            }
            return launcher;
        }

        // POST api/launchers/cscript
        // <summary>
        // Generate CscriptLauncher LauncherString
        // </summary>
        [HttpPost("cscript", Name = "GenerateCscriptLauncher")]
        public ActionResult<CscriptLauncher> GenerateCscriptLauncher()
        {
            CscriptLauncher launcher = (CscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Cscript);
            if (launcher == null)
            {
                return NotFound($"NotFound - CscriptLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/cscript/hosted
        // <summary>
        // Generate a CscriptLauncher that points to a hosted sct file
        // </summary>
        [HttpPost("cscript/hosted", Name = "GenerateCscriptHostedFileLauncher")]
        public ActionResult<CscriptLauncher> GenerateCscriptHostedFileLauncher(HostedFile hostedFile)
        {
            CscriptLauncher launcher = (CscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Cscript);
            if (launcher == null)
            {
                return NotFound($"NotFound - CscriptLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/cscript
        // <summary>
        // Edit CscriptLauncher
        // </summary>
        [HttpPut("cscript", Name = "PutCscriptLauncher")]
        public ActionResult<CscriptLauncher> PutCscriptLauncher([FromBody]CscriptLauncher cscriptLauncher)
        {
            CscriptLauncher launcher = (CscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Cscript);
            if (launcher == null || launcher.Id != cscriptLauncher.Id)
            {
                return NotFound($"NotFound - CscriptLauncher with id: {cscriptLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == cscriptLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = cscriptLauncher.ListenerId;
            }
            launcher.CommType = cscriptLauncher.CommType;
            launcher.SMBPipeName = cscriptLauncher.SMBPipeName;
            launcher.ValidateCert = cscriptLauncher.ValidateCert;
            launcher.UseCertPinning = cscriptLauncher.UseCertPinning;
            launcher.Delay = cscriptLauncher.Delay;
            launcher.JitterPercent = cscriptLauncher.JitterPercent;
            launcher.ConnectAttempts = cscriptLauncher.ConnectAttempts;
            launcher.KillDate = cscriptLauncher.KillDate;
            launcher.ScriptLanguage = cscriptLauncher.ScriptLanguage;
            launcher.DotNetFrameworkVersion = cscriptLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = cscriptLauncher.LauncherString;
            launcher.DiskCode = cscriptLauncher.DiskCode;
            launcher.StagerCode = cscriptLauncher.StagerCode;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // GET api/launchers/wscript
        // <summary>
        // Get WscriptLauncher
        // </summary>
        [HttpGet("wscript", Name = "GetWscriptLauncher")]
        public ActionResult<WscriptLauncher> GetWscriptLauncher()
        {
            WscriptLauncher launcher = (WscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wscript);
            if (launcher == null)
            {
                return NotFound($"NotFound - WscriptLauncher");
            }
            return launcher;
        }

        // POST api/launchers/wscript
        // <summary>
        // Generate WscriptLauncher LauncherString
        // </summary>
        [HttpPost("wscript", Name = "GenerateWscriptLauncher")]
        public ActionResult<WscriptLauncher> GenerateWscriptLauncher()
        {
            WscriptLauncher launcher = (WscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wscript);
            if (launcher == null)
            {
                return NotFound($"NotFound - WscriptLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == launcher.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {launcher.ListenerId}");
            }
            HttpProfile profile = (HttpProfile)_context.Profiles.FirstOrDefault(P => P.Id == listener.ProfileId);
            if (profile == null)
            {
                return NotFound($"NotFound - HttpProfile with id: {listener.ProfileId}");
            }

            Grunt grunt = new Grunt
            {
                ListenerId = listener.Id,
                CovenantIPAddress = listener.BindAddress,
                CommType = launcher.CommType,
                SMBPipeName = launcher.SMBPipeName,
                ValidateCert = launcher.ValidateCert,
                UseCertPinning = launcher.UseCertPinning,
                Delay = launcher.Delay,
                JitterPercent = launcher.JitterPercent,
                ConnectAttempts = launcher.ConnectAttempts,
                KillDate = launcher.KillDate,
                DotNetFrameworkVersion = launcher.DotNetFrameworkVersion
            };

            _context.Grunts.Add(grunt);
            _context.SaveChanges();
            launcher.GetLauncher(listener, grunt, profile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // POST api/launchers/wscript/hosted
        // <summary>
        // Generate a WscriptLauncher that points to a hosted sct file
        // </summary>
        [HttpPost("wscript/hosted", Name = "GenerateWscriptHostedFileLauncher")]
        public ActionResult<WscriptLauncher> GenerateWscriptHostedFileLauncher(HostedFile hostedFile)
        {
            WscriptLauncher launcher = (WscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wscript);
            if (launcher == null)
            {
                return NotFound($"NotFound - WscriptLauncher");
            }
            Listener listener = _context.Listeners.FirstOrDefault(S => S.Id == hostedFile.ListenerId);
            if (listener == null)
            {
                return NotFound($"NotFound - Listener with id: {hostedFile.ListenerId}");
            }
            HostedFile savedHostedFile = _context.HostedFiles.FirstOrDefault(HF => HF.Id == hostedFile.Id);
            if (savedHostedFile == null)
            {
                return NotFound($"NotFound - HostedFile with id: {hostedFile.Id}");
            }
            string hostedLauncher = launcher.GetHostedLauncher(listener, savedHostedFile);
            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }

        // PUT api/launchers/wscript
        // <summary>
        // Edit WscriptLauncher
        // </summary>
        [HttpPut("wscript", Name = "PutWscriptLauncher")]
        public ActionResult<WscriptLauncher> PutWscriptLauncher([FromBody] WscriptLauncher wscriptLauncher)
        {
            WscriptLauncher launcher = (WscriptLauncher)_context.Launchers.FirstOrDefault(S => S.Type == Launcher.LauncherType.Wscript);
            if (launcher == null || launcher.Id != wscriptLauncher.Id)
            {
                return NotFound($"NotFound - WscriptLauncher with id: {wscriptLauncher.Id}");
            }
            Listener listener = _context.Listeners.FirstOrDefault(L => L.Id == wscriptLauncher.ListenerId);
            if (listener != null)
            {
                launcher.ListenerId = wscriptLauncher.ListenerId;
            }
            launcher.CommType = wscriptLauncher.CommType;
            launcher.SMBPipeName = wscriptLauncher.SMBPipeName;
            launcher.ValidateCert = wscriptLauncher.ValidateCert;
            launcher.UseCertPinning = wscriptLauncher.UseCertPinning;
            launcher.Delay = wscriptLauncher.Delay;
            launcher.JitterPercent = wscriptLauncher.JitterPercent;
            launcher.ConnectAttempts = wscriptLauncher.ConnectAttempts;
            launcher.KillDate = wscriptLauncher.KillDate;
            launcher.ScriptLanguage = wscriptLauncher.ScriptLanguage;
            launcher.DotNetFrameworkVersion = wscriptLauncher.DotNetFrameworkVersion;
            launcher.LauncherString = wscriptLauncher.LauncherString;
            launcher.DiskCode = wscriptLauncher.DiskCode;
            launcher.StagerCode = wscriptLauncher.StagerCode;

            _context.Launchers.Update(launcher);
            _context.SaveChanges();

            return launcher;
        }
    }
}
