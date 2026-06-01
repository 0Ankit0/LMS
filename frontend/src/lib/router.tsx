import {
  Link as RouterLink,
  type LinkProps as RouterLinkProps,
  type To,
  useLocation,
  useNavigate,
  useParams as useRouteParams,
  useSearchParams as useRouterSearchParams,
} from 'react-router-dom';

type LinkProps = Omit<RouterLinkProps, 'to'> & {
  href: To;
  replace?: boolean;
  scroll?: boolean;
  prefetch?: boolean;
};

export function Link({ href, replace, ...props }: LinkProps) {
  return <RouterLink to={href} replace={replace} {...props} />;
}

export function useRouter() {
  const navigate = useNavigate();

  return {
    push: (href: string) => navigate(href),
    replace: (href: string) => navigate(href, { replace: true }),
    back: () => navigate(-1),
    forward: () => navigate(1),
    refresh: () => navigate(0),
    prefetch: async () => undefined,
  };
}

export function usePathname() {
  return useLocation().pathname;
}

export function useSearchParams() {
  return useRouterSearchParams()[0];
}

export function useParams<T extends Record<string, string | undefined>>() {
  return useRouteParams() as T;
}