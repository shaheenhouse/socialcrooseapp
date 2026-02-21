import { NextRequest, NextResponse } from 'next/server';
import { generateDesign, isGeminiConfigured } from '@/lib/gemini';

export async function POST(request: NextRequest) {
  try {

    if (!isGeminiConfigured()) {
      return NextResponse.json(
        { error: 'AI is not configured. Please add GEMINI_API_KEY to your .env.local file.' },
        { status: 503 }
      );
    }

    const body = await request.json();
    const { prompt, width, height, referenceImage } = body;

    if (!prompt || !width || !height) {
      return NextResponse.json(
        { error: 'Missing required fields: prompt, width, height' },
        { status: 400 }
      );
    }

    const designJSON = await generateDesign(prompt, width, height, referenceImage);

    return NextResponse.json({ designJSON });
  } catch (error: any) {
    console.error('AI design generation error:', error);
    return NextResponse.json(
      { error: error.message || 'Failed to generate design' },
      { status: 500 }
    );
  }
}
