$framework = '4.0'

properties {
	$base_dir = resolve-path .
	$build_dir = "$base_dir\build"
	$dist_dir = "$base_dir\release"
	$source_dir = "$base_dir\src"
	$tools_dir = "$base_dir\tools"
	$test_dir = "$build_dir\test"
	$result_dir = "$build_dir\results"
	$lib_dir = "$base_dir\lib"
	$path_to_msbuild = "$lib_dir\msbuild\Microsoft.Build.Engine.dll"
	$buildNumber = if ($env:build_number -ne $NULL) { $env:build_number } else { '2.0.9999.0' }
	$config = "debug"
}


task default -depends local
task local -depends compile, test
task full -depends local, merge, dist
task ci -depends clean, commonAssemblyInfo, local, merge, dist

task clean {
	delete_directory "$build_dir"
	delete_directory "$dist_dir"
}

task compile -depends clean { 
    exec { msbuild /t:Clean /t:Build /p:Configuration=Automated$config /v:q /nologo $source_dir\AutoMapper.sln }
}

task commonAssemblyInfo {
    $commit = git log -1 --pretty=format:%H
    create-commonAssemblyInfo "$buildNumber" "$commit" "$source_dir\CommonAssemblyInfo.cs"
}

task merge {
	create_directory "$build_dir\merge"
	exec { & $tools_dir\ILMerge\ilmerge.exe /v2 /log /out:"$build_dir\merge\AutoMapper.dll" /internalize:AutoMapper.exclude "$build_dir\$config\AutoMapper\AutoMapper.dll" "$build_dir\$config\AutoMapper\Castle.Core.dll" "$build_dir\$config\AutoMapper\Castle.DynamicProxy2.dll" /keyfile:"$source_dir\AutoMapper.snk" }
}

task test {
	create_directory "$build_dir\results"
    exec { & $tools_dir\nunit\nunit-console-x86.exe $build_dir/$config/UnitTests/AutoMapper.UnitTests.dll /nologo /nodots /xml=$result_dir\AutoMapper.xml }
}

task dist {
	create_directory $dist_dir
	$exclude = @('*.pdb')
	copy_files "$build_dir\merge" "$build_dir\dist-merged" $exclude
	copy_files "$build_dir\$config\AutoMapper" "$build_dir\dist" $exclude
	zip_directory "$build_dir\dist" "$dist_dir\AutoMapper-unmerged.zip"
	copy-item "$build_dir\dist-merged\AutoMapper.dll" "$dist_dir"
}

# -------------------------------------------------------------------------------------------------------------
# generalized functions 
# --------------------------------------------------------------------------------------------------------------
function global:zip_directory($directory, $file)
{
    delete_file $file
    cd $directory
    exec { & "$tools_dir\7-zip\7za.exe" a $file *.* }
    cd $base_dir
}

function global:delete_directory($directory_name)
{
  rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:delete_file($file)
{
    if($file) {
        remove-item $file  -force  -ErrorAction SilentlyContinue | out-null} 
}

function global:create_directory($directory_name)
{
  mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:copy_files($source, $destination, $exclude = @()) {
    create_directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
}

function global:run_nunit ($test_assembly)
{
    exec { & $tools_dir\nunit\nunit-console-x86.exe $test_dir$test_assembly /nologo /nodots /xml=$result_dir$test_assembly.xml }
}

function global:create-commonAssemblyInfo($version, $commit, $filename)
{
	$date = Get-Date
    "using System;
using System.Reflection;
using System.Runtime.InteropServices;

//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4927
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: ComVisibleAttribute(false)]
[assembly: AssemblyVersionAttribute(""$version"")]
[assembly: AssemblyFileVersionAttribute(""$version"")]
[assembly: AssemblyCopyrightAttribute(""Copyright Jimmy Bogard 2008-" + $date.Year + """)]
[assembly: AssemblyProductAttribute(""AutoMapper"")]
[assembly: AssemblyTrademarkAttribute(""$commit"")]
[assembly: AssemblyCompanyAttribute("""")]
[assembly: AssemblyConfigurationAttribute(""release"")]
[assembly: AssemblyInformationalVersionAttribute(""$version"")]"  | out-file $filename -encoding "ASCII"    
}