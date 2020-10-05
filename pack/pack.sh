# Initial cleanup
rm -rf initial
rm -rf unpacked
rm -f NScatterGather.nupkg

# Create base nupkg from project w/ properties.
dotnet pack ../src/NScatterGather/NScatterGather.csproj -o "initial" --verbosity quiet
echo 'Initial nupkg created from project'

# Unpack the nupkg
unzip -qq initial/* -d unpacked/
echo '  initial nupkg unpacked'

# Make files writable
chmod 666 -R unpacked

echo 'Adding tags to nuspec'

# Add <owners>
xmlstarlet ed --inplace -a "/_:package/_:metadata/_:authors" -t elem -n "owners" -v "tommasobertoni" unpacked/NScatterGather.nuspec
echo '  added <owners> tag'

# Add <licenseUrl>
xmlstarlet ed --inplace -a "/_:package/_:metadata/_:requireLicenseAcceptance" -t elem -n "licenseUrl" -v "https://aka.ms/deprecateLicenseUrl" unpacked/NScatterGather.nuspec
echo '  added <licenseUrl> tag [deprecated]'

# Add <license>
xmlstarlet ed --inplace -a "/_:package/_:metadata/_:licenseUrl" -t elem -n "license" -v "LICENSE" unpacked/NScatterGather.nuspec
echo '  added <license> tag'
xmlstarlet ed --inplace -s "/_:package/_:metadata/_:license" -t attr -n 'type' -v 'file' unpacked/NScatterGather.nuspec
echo '    set type="file" attribute to <license> tag'

# Copy LICENSE
cp --remove-destination ../LICENSE unpacked/LICENSE
echo '    copied LICENSE into target folder'

# Add <icon>
xmlstarlet ed --inplace -a "/_:package/_:metadata/_:tags" -t elem -n "icon" -v "nscattergather-logo.png" unpacked/NScatterGather.nuspec
echo '  added <icon> tag'

# Copy icon
cp --remove-destination ../assets/logo/nscattergather-logo-128.png unpacked/nscattergather-logo.png
echo '    copied logo into target folder'

echo 'Creating final nupkg'

# Zip final nupkg
cd unpacked; zip -rq ../NScatterGather.nupkg *; cd ..
echo '  completed'

# Final cleanup
rm -rf initial
rm -rf unpacked
