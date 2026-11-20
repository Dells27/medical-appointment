"use client";

import { useRouter } from "next/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { logoutRequest } from "@/lib/api/auth";
import { useAppDispatch } from "@/store/hooks";
import { logout } from "@/store/slices/authslices";

// Clears the httpOnly cookie on the server, the cached queries and the Redux session
export function useLogout() {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const queryClient = useQueryClient();

  return async function handleLogout() {
    try {
      await logoutRequest();
    } catch {
      // Even if the request fails, end the session locally
    }
    queryClient.clear();
    dispatch(logout());
    router.push("/login");
  };
}
