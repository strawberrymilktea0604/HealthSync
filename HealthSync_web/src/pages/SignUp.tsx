import React, { useMemo, useRef, useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../constant/apiService";
import { toast } from "react-toastify";

const bgImage = new URL("../assets/anhnen.jpg", import.meta.url).href;

export default function SignUp() {
  const nav = useNavigate();

  // form state
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [pw, setPw] = useState("");
  const [pw2, setPw2] = useState("");
  const [agree, setAgree] = useState(false);

  // ui state
  const [showPw, setShowPw] = useState(false);
  const [showPw2, setShowPw2] = useState(false);
  const [touched, setTouched] = useState<Record<string, boolean>>({});
  const [submitted, setSubmitted] = useState(false);
  const [loading, setLoading] = useState(false);
  const [apiError, setApiError] = useState<string | null>(null);

  // refs từng field
  const nameRef = useRef<HTMLInputElement>(null);
  const emailRef = useRef<HTMLInputElement>(null);
  const pwRef = useRef<HTMLInputElement>(null);
  const pw2Ref = useRef<HTMLInputElement>(null);
  const agreeRef = useRef<HTMLInputElement>(null);

  // validation
  const emailOk = useMemo(() => /.+@.+\..+/.test(email), [email]);
  const strength = useMemo(() => {
    let s = 0;
    if (pw.length >= 8) s++;
    if (/[A-Z]/.test(pw)) s++;
    if (/[a-z]/.test(pw)) s++;
    if (/[0-9]/.test(pw)) s++;
    if (/[^A-Za-z0-9]/.test(pw)) s++;
    return s; // 0..5
  }, [pw]);
  const pwOk = pw.length >= 8 && strength >= 3;
  const matchOk = pw && pw === pw2;

  const errors: Record<string, string> = {
    ...(name.trim().length < 2 ? { name: "Họ và tên tối thiểu 2 ký tự." } : {}),
    ...(!emailOk ? { email: "Email không hợp lệ." } : {}),
    ...(!pwOk
      ? { pw: "Mật khẩu ≥ 8 ký tự và nên có chữ hoa, chữ thường, số/ký tự đặc biệt." }
      : {}),
    ...(!matchOk ? { pw2: "Xác nhận mật khẩu chưa khớp." } : {}),
    ...(!agree ? { agree: "Bạn cần đồng ý Điều khoản & Chính sách." } : {}),
  };
  const isValid = Object.keys(errors).length === 0;
  const showError = (key: string) => (submitted || touched[key]) && !!errors[key];
  const markTouched = (key: string) => setTouched((t) => ({ ...t, [key]: true }));

  function focusFirstError() {
    if (errors.name) return nameRef.current?.focus();
    if (errors.email) return emailRef.current?.focus();
    if (errors.pw) return pwRef.current?.focus();
    if (errors.pw2) return pw2Ref.current?.focus();
    if (errors.agree) return agreeRef.current?.focus();
  }

  async function onSubmit(e: React.FormEvent) {
    e.preventDefault();
    setSubmitted(true);
    setApiError(null);

    if (!isValid) {
      focusFirstError();
      return;
    }

    setLoading(true);
    try {
      await register({
        email,
        password: pw,
        fullName: name,
        dateOfBirth: new Date().toISOString(),
        gender: "other",
        heightCm: 170,
        weightKg: 70,
      });

      toast.success("Đăng ký thành công! Vui lòng đăng nhập.");
      nav("/login", { replace: true });
    } catch (err: any) {
      const msg =
        err?.response?.data?.message ||
        err?.message ||
        "Đăng ký thất bại. Vui lòng thử lại.";

      setApiError(msg);
      toast.error(msg);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div
      className="relative h-screen w-full bg-cover bg-center bg-no-repeat"
      style={{ backgroundImage: `url(${bgImage})` }}
    >
      <div className="absolute inset-0 bg-black/25" />

      <div className="relative z-10 flex h-screen w-full items-center justify-center px-4">
        <div className="w-full max-w-lg rounded-3xl border border-zinc-200 bg-white p-6 sm:p-8 shadow-2xl text-zinc-900">
          {/* Header */}
          <div className="mb-6 flex items-center justify-between">
            <button
              onClick={() => (window.history.length > 1 ? nav(-1) : nav("/"))}
              className="inline-flex items-center gap-2 rounded-xl border border-zinc-300 bg-white px-3 py-2 text-sm text-zinc-700 hover:bg-zinc-100 transition"
            >
              <ArrowLeft className="h-4 w-4" />
              Quay lại
            </button>
            <div className="font-extrabold">
              Health<span className="text-emerald-600">Sync</span>
            </div>
          </div>

          <h1 className="text-xl font-semibold">Tạo tài khoản</h1>
          <p className="mt-1 text-sm text-zinc-600">Điền thông tin của bạn để bắt đầu</p>

          <form onSubmit={onSubmit} className="mt-6 space-y-4" noValidate>
            {/* Họ và tên */}
            <div>
              <label htmlFor="name" className="mb-1 block text-sm font-medium">
                Họ và tên
              </label>
              <input
                ref={nameRef}
                id="name"
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                onBlur={() => markTouched("name")}
                className={`w-full rounded-xl border px-4 py-3 outline-none focus:ring-4 transition ${
                  showError("name")
                    ? "border-red-500 ring-red-500/20"
                    : "border-zinc-300 focus:border-emerald-600 ring-emerald-500/20"
                }`}
                placeholder="Nguyễn Văn A"
                autoFocus
              />
              {showError("name") && (
                <p className="mt-1 text-xs text-red-600">{errors.name}</p>
              )}
            </div>

            {/* Email */}
            <div>
              <label htmlFor="email" className="mb-1 block text-sm font-medium">
                Email
              </label>
              <input
                ref={emailRef}
                id="email"
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                onBlur={() => markTouched("email")}
                className={`w-full rounded-xl border px-4 py-3 outline-none focus:ring-4 transition ${
                  showError("email")
                    ? "border-red-500 ring-red-500/20"
                    : email
                    ? "border-emerald-500 focus:border-emerald-600 ring-emerald-500/20"
                    : "border-zinc-300 focus:border-emerald-600 ring-emerald-500/20"
                }`}
                placeholder="you@example.com"
                autoComplete="email"
              />
              {showError("email") && (
                <p className="mt-1 text-xs text-red-600">{errors.email}</p>
              )}
            </div>

            {/* Mật khẩu */}
            <div>
              <label htmlFor="pw" className="mb-1 block text-sm font-medium">
                Mật khẩu
              </label>
              <div className="relative">
                <input
                  ref={pwRef}
                  id="pw"
                  type={showPw ? "text" : "password"}
                  value={pw}
                  onChange={(e) => setPw(e.target.value)}
                  onBlur={() => markTouched("pw")}
                  className={`w-full rounded-xl border px-4 py-3 pr-12 outline-none focus:ring-4 transition ${
                    showError("pw")
                      ? "border-red-500 ring-red-500/20"
                      : "border-zinc-300 focus:border-emerald-600 ring-emerald-500/20"
                  }`}
                  placeholder="Tối thiểu 8 ký tự"
                  autoComplete="new-password"
                  aria-invalid={!!showError("pw")}
                />
                <button
                  type="button"
                  onClick={() => setShowPw((s) => !s)}
                  aria-label={showPw ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                  className="absolute inset-y-0 right-0 grid place-items-center px-3 text-zinc-900 hover:text-zinc-950"
                >
                  {showPw ? <Eye /> : <EyeOff />}
                </button>
              </div>

              {/* Strength bar */}
              <div className="mt-2 h-2 w-full rounded-full bg-zinc-200 overflow-hidden">
                <div
                  className={`h-full transition-all ${
                    strength >= 4
                      ? "bg-emerald-600 w-11/12"
                      : strength === 3
                      ? "bg-emerald-500 w-8/12"
                      : strength === 2
                      ? "bg-yellow-500 w-5/12"
                      : strength === 1
                      ? "bg-red-500 w-3/12"
                      : "bg-transparent w-0"
                  }`}
                />
              </div>
              <p className="mt-1 text-xs text-zinc-500">
                Nên có chữ hoa, chữ thường, số và ký tự đặc biệt.
              </p>
              {showError("pw") && (
                <p className="mt-1 text-xs text-red-600">{errors.pw}</p>
              )}
            </div>

            {/* Xác nhận mật khẩu */}
            <div>
              <label htmlFor="pw2" className="mb-1 block text-sm font-medium">
                Xác nhận mật khẩu
              </label>
              <div className="relative">
                <input
                  ref={pw2Ref}
                  id="pw2"
                  type={showPw2 ? "text" : "password"}
                  value={pw2}
                  onChange={(e) => setPw2(e.target.value)}
                  onBlur={() => markTouched("pw2")}
                  className={`w-full rounded-xl border px-4 py-3 pr-12 outline-none focus:ring-4 transition ${
                    showError("pw2")
                      ? "border-red-500 ring-red-500/20"
                      : pw2
                      ? "border-emerald-500 focus:border-emerald-600 ring-emerald-500/20"
                      : "border-zinc-300 focus:border-emerald-600 ring-emerald-500/20"
                  }`}
                  placeholder="Nhập lại mật khẩu"
                  autoComplete="new-password"
                />
                <button
                  type="button"
                  onClick={() => setShowPw2((s) => !s)}
                  aria-label={showPw2 ? "Ẩn mật khẩu" : "Hiện mật khẩu"}
                  className="absolute inset-y-0 right-0 grid place-items-center px-3 text-zinc-900 hover:text-zinc-950"
                >
                  {showPw2 ? <Eye /> : <EyeOff />}
                </button>
              </div>
              {showError("pw2") && (
                <p className="mt-1 text-xs text-red-600">{errors.pw2}</p>
              )}
            </div>

            {/* Đồng ý điều khoản */}
            <div className="flex items-center gap-2 text-sm text-zinc-700">
              <input
                ref={agreeRef}
                id="agree"
                type="checkbox"
                checked={agree}
                onChange={(e) => setAgree(e.target.checked)}
                onBlur={() => markTouched("agree")}
                className={`h-4 w-4 rounded border ${
                  showError("agree") ? "border-red-500" : "border-zinc-300"
                }`}
              />
              <label htmlFor="agree">
                Tôi đồng ý với{" "}
                <a href="#terms" className="text-emerald-700 hover:underline">
                  Điều khoản
                </a>{" "}
                và{" "}
                <a href="#privacy" className="text-emerald-700 hover:underline">
                  Chính sách
                </a>
              </label>
            </div>
            {showError("agree") && (
              <p className="mt-1 text-xs text-red-600">{errors.agree}</p>
            )}

            {/* Nút đăng ký: chữ xanh đậm, nền trắng */}
            <button
              type="submit"
              disabled={loading}
              className="grid w-full place-items-center rounded-xl px-4 py-3 font-semibold
                         text-emerald-700 bg-white border border-emerald-600 shadow-sm
                         hover:bg-emerald-50 focus:outline-none focus:ring-4 focus:ring-emerald-500/20
                         disabled:cursor-not-allowed"
            >
              {loading ? "Đang tạo tài khoản…" : "Đăng ký"}
            </button>

            {/* Lỗi API dưới nút */}
            {apiError && (
              <div className="text-sm text-red-600" role="alert">
                {apiError}
              </div>
            )}
          </form>

          <div className="mt-6 text-center text-sm text-zinc-600">
            Đã có tài khoản?{" "}
            <Link to="/login" className="text-emerald-700 hover:underline">
              Đăng nhập
            </Link>
          </div>

          <p className="mt-6 text-center text-xs text-zinc-500">
            © {new Date().getFullYear()} HealthSync
          </p>
        </div>
      </div>
    </div>
  );
}

function ArrowLeft(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.2} className={props.className}>
      <path d="M15 18l-6-6 6-6" />
    </svg>
  );
}
function Eye(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
         strokeWidth={2.2} className={`h-5 w-5 ${props.className || ""}`}>
      <path d="M2 12s3.8-7 10-7 10 7 10 7-3.8 7-10 7S2 12 2 12Z" />
      <circle cx="12" cy="12" r="3.2" />
    </svg>
  );
}
function EyeOff(props: React.SVGProps<SVGSVGElement>) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"
         strokeWidth={2.2} className={`h-5 w-5 ${props.className || ""}`}>
      <path d="M3 3l18 18" />
      <path d="M10.6 5.1A9.8 9.8 0 0112 5c6.2 0 10 7 10 7a17.3 17.3 0 01-3.3 4.3M7.5 7.8C4.3 9.8 2 12 2 12s3.8 7 10 7a9.9 9.9 0 003.4-.6" />
      <path d="M9.9 9.9a3.5 3.5 0 004.2 4.2" />
    </svg>
  );
}
