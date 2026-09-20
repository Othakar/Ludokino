import { NextRequest, NextResponse } from "next/server";

const allowedPublicPath = /^\/img\/[a-zA-Z0-9._/-]+$/;
const blockedPath = /(?:\.\.|%2e|%2f|%5c|\\|^\/\.(?!well-known))/i;

export function proxy(request: NextRequest) {
  const pathname = request.nextUrl.pathname;

  if (blockedPath.test(pathname)) {
    return new NextResponse("Bad Request", { status: 400 });
  }

  if (pathname.startsWith("/img/") && !allowedPublicPath.test(pathname)) {
    return new NextResponse("Not Found", { status: 404 });
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/((?!_next/static|_next/image|favicon.ico).*)"],
};
