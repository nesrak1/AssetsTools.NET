add_library (TextureEncoder SHARED "TextureEncoder.cpp" "TextureEncoder.h" "TextureFormat.h")
set_target_properties(TextureEncoder PROPERTIES OUTPUT_NAME "textureencoder")

if (CMAKE_VERSION VERSION_GREATER 3.12)
  set_property(TARGET TextureEncoder PROPERTY CXX_STANDARD 20)
endif()

if (WIN32)
    # Windows specific settings
    set(LIB_DIR "${CMAKE_SOURCE_DIR}/NativeLibs/cuttlefish/windows/lib")
    set(BIN_DIR "${CMAKE_SOURCE_DIR}/NativeLibs/cuttlefish/windows/bin")
    set(LIB_PREFIX "")
    set(LIB_SUFFIX ".lib")
else()
    # Linux specific settings
    set(LIB_DIR "${CMAKE_SOURCE_DIR}/NativeLibs/cuttlefish/linux/lib")
    set(BIN_DIR "${CMAKE_SOURCE_DIR}/NativeLibs/cuttlefish/linux/bin")
    set(LIB_PREFIX "lib")
    set(LIB_SUFFIX ".so")
endif()

link_directories(${LIB_DIR})
target_include_directories(TextureEncoder PRIVATE "${CMAKE_SOURCE_DIR}/NativeLibs/cuttlefish/windows/include")

target_link_libraries(TextureEncoder ${LIB_DIR}/${LIB_PREFIX}cuttlefish${LIB_SUFFIX})
if (PVRTL_ENABLED)
    target_link_libraries(TextureEncoder ${LIB_DIR}/${LIB_PREFIX}PVRTexLib${LIB_SUFFIX})
endif()

if (WIN32)
    set(DEP_DLLS_TO_BIN "cuttlefish.dll")
    if(PVRTL_ENABLED)
      list(APPEND DEP_DLLS_TO_BIN "PVRTexLib.dll")
    endif()
    foreach(DLL ${DEP_DLLS_TO_BIN})
        add_custom_command(TARGET TextureEncoder POST_BUILD
            COMMAND ${CMAKE_COMMAND} -E copy_if_different
            "${BIN_DIR}/${DLL}"
            $<TARGET_FILE_DIR:TextureEncoder>)
    endforeach()
endif()