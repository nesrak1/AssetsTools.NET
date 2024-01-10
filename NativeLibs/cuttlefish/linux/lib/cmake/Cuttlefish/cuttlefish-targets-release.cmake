#----------------------------------------------------------------
# Generated CMake target import file for configuration "Release".
#----------------------------------------------------------------

# Commands may need to know the format version.
set(CMAKE_IMPORT_FILE_VERSION 1)

# Import target "Cuttlefish::tool" for configuration "Release"
set_property(TARGET Cuttlefish::tool APPEND PROPERTY IMPORTED_CONFIGURATIONS RELEASE)
set_target_properties(Cuttlefish::tool PROPERTIES
  IMPORTED_LOCATION_RELEASE "${_IMPORT_PREFIX}/bin/cuttlefish.exe"
  )

list(APPEND _IMPORT_CHECK_TARGETS Cuttlefish::tool )
list(APPEND _IMPORT_CHECK_FILES_FOR_Cuttlefish::tool "${_IMPORT_PREFIX}/bin/cuttlefish.exe" )

# Commands beyond this point should not need to know the version.
set(CMAKE_IMPORT_FILE_VERSION)
