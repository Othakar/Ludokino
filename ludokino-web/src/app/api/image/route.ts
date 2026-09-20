import { NextRequest, NextResponse } from "next/server";

const allowedHosts = new Set(["i.imgur.com", "img.youtube.com", "i.ytimg.com"]);

export async function GET(request: NextRequest) {
  const source = request.nextUrl.searchParams.get("url");
  if (!source) return new NextResponse("Missing image URL", { status: 400 });

  let url: URL;
  try {
    url = new URL(source);
  } catch {
    return new NextResponse("Invalid image URL", { status: 400 });
  }

  if (url.protocol !== "https:" || !allowedHosts.has(url.hostname)) {
    return new NextResponse("Image host not allowed", { status: 403 });
  }

  const response = await fetch(url, { next: { revalidate: 3600 } });
  if (!response.ok) return new NextResponse("Image unavailable", { status: response.status });

  return new NextResponse(response.body, {
    headers: {
      "Cache-Control": "public, max-age=3600, s-maxage=86400",
      "Content-Type": response.headers.get("content-type") ?? "image/jpeg",
    },
  });
}
