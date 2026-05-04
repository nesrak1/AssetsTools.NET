#----------------------------------------------------------------
# Generated CMake target import file for configuration "Debug".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "Cuttlefish::lib" for configuration "Debug"
set_property(TARGET Cuttlefish::lib APPEND PROPERTY IMPORTED_CONFIGURATIONS DEBUG)
set_target_properties(Cuttlefish::lib PROPERTIES
  IMPORTED_IMPLIB_DEBUG "${_IMPORT_PREFIX}/lib/cuttlefishd.lib"
  IMPORTED_LOCATION_DEBUG "${_IMPORT_PREFIX}/bin/cuttlefishd.dll"
  )

list(APPEND _cmake_import_check_targets Cuttlefish::lib )
list(APPEND _cmake_import_check_files_for_Cuttlefish::lib "${_IMPORT_PREFIX}/lib/cuttlefishd.lib" "${_IMPORT_PREFIX}/bin/cuttlefishd.dll" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
