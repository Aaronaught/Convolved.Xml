Convolved.Xml
=============

Purpose
-------
I wrote this library to assist myself and any other unfortunate souls who are still depending on the legacy "web reference" system, e.g. due to interopping with older WSE-based services. If you don't do any of that, you won't be interested in this project right now.

What is it?
-----------
Essentially, it's a replacement for [Jelle Druyt's Schema Importer Extension](http://www.microsoft.com/belux/msdn/fr/community/columns/jdruyts/wsproxy.mspx) from all the way back in 2005. An enterprising developer [fixed some bugs with it](http://www.alexthissen.nl/blogs/main/archive/2006/07/24/fixing-the-shared-type-schema-importer-extension-for-nullable-value-types.aspx) in 2006, but it remains a maintenance headache.

This solution does not require any fiddling with machine.config or adding shared type assemblies into the GAC. It operates on the principle of Convention over Configuration; just point it to your build drop for the shared types and it will scan all the assemblies and figure out which types it can use for the proxy.

How do I use it?
----------------
First, you'll still need to install the extension itself into the GAC. The WiX installer takes care of this. You'll just need [Votive](http://wix.sourceforge.net/votive.html) to build the installer. The project does not have any other dependencies.

Second, you'll need to create a WSDL parameters file. You can do probably do this just once and then reuse the same prameters file everywhere. This should be familiar to anyone already using these legacy tools:

    <wsdlParameters xmlns="http://microsoft.com/webReference/">
      <nologo>true</nologo>
      <sharetypes>true</sharetypes>
      <webReferenceOptions>
        <schemaImporterExtension>
          <type>Convolved.Xml.Serialization.TypeDiscoveryExtension,Convolved.Xml.Serialization</type>
        </schemaImporterExtensions>
        <style>client</style>
      </webReferenceOptions>
    </wsdlParameters>
	
Last but not least, use the [wsdl command-line tool](http://msdn.microsoft.com/en-us/library/7h3ystb6.aspx) to generate the proxy. The extension uses an environment variable called `ConvolvedXmlDiscoveryPath` to determine the probing path; if not specified, it will use the current working directory. So a simple "script" might look like:

    SET ConvolvedXmlDiscoveryPath=C:\Projects\MyProject\FooIntegration\bin
	wsdl /out:C:\Projects\MyProject\FooIntegration\FooService.cs /par:FooParameters.xml http://example.com/foo/service.asmx?wsdl
	
If you need to interop using WSE and want to get really fancy, you could put this into a PowerShell script and add a couple of lines to replace the base type, using the method described by the [Scripting Guy](http://blogs.technet.com/b/heyscriptingguy/archive/2008/01/17/how-can-i-use-windows-powershell-to-replace-characters-in-a-text-file.aspx):

    (Get-Content C:\Projects\MyProject\FooIntegration\FooService.cs) | 
    Foreach-Object {$_ -replace "System\.Web\.Services\.SoapHttpClientProtocol", "Microsoft.Web.Services2.WebServicesClientProtocol"} | 
    Set-Content C:\Projects\MyProject\FooIntegration\FooService.cs

This is particularly helpful if, like most of us, you upgraded to Visual Studio 2010 *in 2010* and don't want to be stuck building your integration libraries in VS 2005 until the end of time.

Enjoy your *almost* painless web references!