require "json"

package = JSON.parse(File.read(File.join(__dir__, "package.json")))

Pod::Spec.new do |s|
  s.name         = "react-native-unity-view"
  s.version      = package["version"]
  s.summary      = package["description"]
  s.homepage     = package["homepage"]
  s.license      = package["license"]
  s.authors      = package["author"]

  s.platforms    = { :ios => "10.0" }
  s.source       = { :git => "https://github.com/brice-gros/react-native-unity-view.git", :tag => "#{s.version}" }

  s.source_files = "ios/**/*.{h,m,mm}"

  s.dependency "React-Core"
  
  #s.frameworks = 'UnityFramework'
  ## Added vendored properties to help locate the name of the framework
  #s.vendored_frameworks = 'Uni	tyFramework.framework'
  #s.vendored_libraries = 'UnityFramework'
  s.xcconfig = {
    'HEADER_SEARCH_PATHS' => '"${PODS_ROOT}/../unityLibrary" "${PODS_ROOT}/../unityLibrary/Classes" "${PODS_ROOT}/../unityLibrary/Classes/Native" "${PODS_ROOT}/../unityLibrary/Classes/PluginBase" "${PODS_ROOT}/../unityLibrary/Classes/UI" "${PODS_ROOT}/../unityLibrary/Classes/Unity" "${PODS_CONFIGURATION_BUILD_DIR}"',
 }
end
