import React, { useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { login } from "../constant/apiService";

const bgImage = new URL("../assets/anhnen.jpg", import.meta.url).href;

export default function Login() {
  const [email, setEmail] = useState("");
  const [pw, setPw] = useState("");
  const [showPw, setShowPw] = useState(false);
  const [loading, setLoading] = useState(false);
  const nav = useNavigate();

  const isValid = useMemo(() => /.+@.+\..+/.test(email) && pw.length >= 6, [email, pw]);
  const [error, setError] = useState<string | null>(null);

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!isValid) return;

    setError(null);
    setLoading(true);

    try {
      const res = await login({ email, password: pw });

      if ((res as any).token) {
        localStorage.setItem("token", (res as any).token);

        toast.success("Đăng nhập thành công!");
        nav("/dashboard", { replace: true });
      } else {
        const msg =
          (res as any).message || "Đăng nhập thành công nhưng không nhận được token.";
        toast.warn(msg);
        setError(msg);
      }
    } catch (err: any) {
      console.error("Lỗi đăng nhập:", err);

      let errorMessage = "Có lỗi xảy ra, vui lòng thử lại.";
      if (err.status === 401 || err.status === 400) {
        errorMessage =
          err.data?.error ||
          err.data?.message ||
          "Email hoặc mật khẩu không chính xác.";
      } else if (err.status > 0) {
        errorMessage = `Lỗi Server (${err.status}): Không thể kết nối.`;
      } else {
        errorMessage =
          "Không thể kết nối đến máy chủ API. Vui lòng kiểm tra kết nối mạng.";
      }

      toast.error(errorMessage);
      setError(errorMessage);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div
      className="relative h-screen w-full bg-cover bg-center bg-no-repeat"
      style={{ backgroundImage: `url(${bgImage})` }}
    >
      <div className="absolute inset-0 bg-black/20" />

      <div className="relative z-10 flex h-screen w-full items-center justify-center px-4">
        <div className="w-full max-w-md rounded-3xl border border-zinc-200 bg-white p-6 sm:p-8 shadow-2xl text-zinc-900">
          {/* Header */}
          <div className="mb-5 flex items-center justify-between">
            <button
              onClick={() => (window.history.length > 1 ? nav(-1) : nav("/"))}
              className="inline-flex items-center gap-2 rounded-xl border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-700 hover:bg-zinc-100 transition"
            >
              <svg
                viewBox="0 0 24 24"
                className="h-4 w-4"
                fill="none"
                stroke="currentColor"
                strokeWidth="1.8"
              >
                <path d="M15 18l-6-6 6-6" />
              </svg>
              Quay lại
            </button>
            <div className="font-extrabold">
              Health<span className="text-emerald-600">Sync</span>
            </div>
          </div>

          <h1 className="text-xl font-semibold">Đăng nhập</h1>
          <p className="mt-1 text-sm text-zinc-600">
            Nhập thông tin tài khoản của bạn
          </p>

          <form onSubmit={onSubmit} className="mt-6 space-y-4">
            {/* Email */}
            <div>
              <label
                htmlFor="email"
                className="mb-1 block text-sm font-medium"
              >
                Email
              </label>
              <input
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="w-full rounded-xl border border-zinc-300 px-4 py-3 outline-none focus:border-emerald-600 focus:ring-4 ring-emerald-500/20 transition"
                placeholder="you@example.com"
                autoComplete="email"
              />
            </div>

            {/* Mật khẩu */}
            <div>
              <div className="mb-1 flex items-center justify-between">
                <label
                  htmlFor="pw"
                  className="block text-sm font-medium"
                >
                  Mật khẩu
                </label>
                <a
                  className="text-sm text-emerald-700 hover:underline"
                  href="/forgot-password"
                >
                  Quên mật khẩu?
                </a>
              </div>

              <div className="relative">
                <input
                  id="pw"
                  type={showPw ? "text" : "password"}
                  value={pw}
                  onChange={(e) => setPw(e.target.value)}
                  className="w-full rounded-xl border border-zinc-300 px-4 py-3 pr-12 outline-none focus:border-emerald-600 focus:ring-4 ring-emerald-500/20 transition"
                  placeholder="••••••••"
                  autoComplete="current-password"
                />
                <button
                  type="button"
                  onClick={() => setShowPw((s) => !s)}
                  aria-label={showPw ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                  className="absolute inset-y-0 right-0 grid place-items-center px-3 text-zinc-700 hover:text-zinc-900"
                >
                  {showPw ? (
                    <svg
                      viewBox="0 0 24 24"
                      className="h-5 w-5"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.8"
                    >
                      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12Z" />
                      <circle cx="12" cy="12" r="3.5" />
                    </svg>
                  ) : (
                    <svg
                      viewBox="0 0 24 24"
                      className="h-5 w-5"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.8"
                    >
                      <path d="M3 3l18 18" />
                      <path d="M10.6 5.1A9.77 9.77 0 0112 5c6.5 0 10 7 10 7a17.4 17.4 0 01-3.3 4.3M7.5 7.8C4.3 9.8 2 12 2 12s3.5 7 10 7a9.9 9.9 0 003.4-.6" />
                      <path d="M9.9 9.9a3.5 3.5 0 004.2 4.2" />
                    </svg>
                  )}
                </button>
              </div>
            </div>

            {/* Nút Đăng nhập */}
            <button
              type="submit"
              disabled={!isValid || loading}
              className="grid w-full place-items-center rounded-xl 
             border border-emerald-600 bg-white px-4 py-3 font-semibold text-emerald-700 
             hover:bg-emerald-50 transition 
             disabled:cursor-not-allowed disabled:bg-emerald-100"
            >
              {loading ? "Đang xử lý…" : "Đăng nhập"}
            </button>



            {error && (
              <div className="mt-2 text-sm text-red-600" role="alert">
                {error}
              </div>
            )}
          </form>

          {/* Đăng ký */}
          <div className="mt-6 text-center text-sm text-zinc-600">
            Chưa có tài khoản?{" "}
            <a
              href="/signup"
              className="text-emerald-700 hover:underline"
            >
              Đăng ký
            </a>
          </div>

          <p className="mt-6 text-center text-xs text-zinc-500">
            © {new Date().getFullYear()} HealthSync
          </p>
        </div>
      </div>
    </div>
  );
}
