# Disconnected RSAT

## Introduction

Disconnected RSAT is a launcher for the official Group Policy Manager, Certificate Authority and Certificate Templates snap-in to bypass the domain joined requirement that is needed when using the official MMC snap-in.

The tool works by injecting a C# library into MMC that will hook the various API calls to trick MMC into believing that the logged on user is a domain user. Hooks are also placed on the `NtCreateFile` API to redirect file paths that would typically be resolved via DFS to a specific domain controller.

## Prerequisites

Since Disconnected RSAT relies on the official snap-ins, you'll first need to install the Windows Remote Server Administration Tools (RSAT) on the non domain joined host you'll be operating from.

## Usage

`mmc.exe` is marked for auto elevation, therefore launching of `DRSAT.exe` should be performed from an elevated command prompt that has either got a relevant TGT with correct permissions imported into the same luid session or alternatively the session has been created using `runas /netonly`. This will ensure that the relevant Kerberos tickets will be fetched automatically or NTLM credentials are used for outbound network connections when `runas /netonly` has been used.

### Launching ADSI Edit

To launch ADSI Edit to target a specific Active Directory domain, simply supply the DNS domain name of the target. Some features of this snap-in (for example, Security tab) don't work properly without using DRSAT. 

```
DRSAT adsi ad.target.com
```

### Launching Active Directory Users and Computers

To launch ADUC to target a specific Active Directory domain, simply supply the DNS domain name of the target. Some features of this snap-in (for example, Security tab) don't work properly without using DRSAT. 

```
DRSAT aduc ad.target.com
```

### Launching Group Policy Management

To launch GPM to target a specific Active Directory domain, simply supply the DNS domain name of the target.

```
DRSAT gpo ad.target.com
```

### Launching DNS Manager

To launch DNS Manager to target a specific Active Directory domain, simply supply the DNS domain name of the target.

```
DRSAT dns ad.target.com
```

### Launching Certification Authority

Whilst the Certification Authority snap-in works when disconnected from the domain, template resolution doesn't work correctly, this can be solved by launching via DRSAT.

```
DRSAT cert ad.target.com
```

### Launching Certificate Template Editor

You can also directly edit certificate templates by using the following command.

```
DRSAT template ad.target.com
```

### Launching custom snap-ins

If there is no snap-in you need, you can specify the necessary one by using the following command.

```
DRSAT custom ad.target.com C:\Windows\System32\custom.msc
```

### Release

Precompiled binaries can be found on the [Releases](https://github.com/CCob/DRSAT/releases) page.
