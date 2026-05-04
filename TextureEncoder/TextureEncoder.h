#pragma once

#include <iostream>

enum class TextureEncoderError
{
    EncodeTexture = -1,
    EncodeMipmaps = -2,
    AllocateMemory = -3
};
