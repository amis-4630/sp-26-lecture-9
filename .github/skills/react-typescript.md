---
name: "React / TypeScript Frontend"
description: "Component patterns, state management with Context + useReducer, form handling, data fetching, and styling conventions for React/TypeScript frontends in this workspace."
---

# React / TypeScript Frontend Skill

Use this skill when creating or modifying React/TypeScript frontend code in this workspace.

---

## Project Structure

```
src/
├── components/        # UI components (one per file)
├── contexts/         # Context providers + custom hooks
├── reducers/         # Reducer functions (pure)
├── types/            # TypeScript type/interface definitions
├── data/             # API fetch functions + type exports
├── assets/           # Static images, icons
├── App.tsx           # Root routing component
├── main.tsx          # Entry point (renders <App />)
└── App.css / index.css
```

---

## State Management: Context + useReducer

This workspace uses React Context with `useReducer` for shared state. Do NOT introduce Redux, Zustand, or other state libraries.

### Context Provider Pattern

```typescript
type MyContextType = {
  state: MyState;
  dispatch: React.Dispatch<MyAction>;
  // Derived/computed values go here
  filteredItems: Item[];
};

const MyContext = createContext<MyContextType | null>(null);

export function MyProvider({ children }: { children: ReactNode }) {
  const [state, dispatch] = useReducer(myReducer, initialState);

  // Compute derived values in the provider, not in components
  const filteredItems = state.filter === "All"
    ? state.items
    : state.items.filter(i => i.category === state.filter);

  return (
    <MyContext.Provider value={{ state, dispatch, filteredItems }}>
      {children}
    </MyContext.Provider>
  );
}
```

### Custom Hook Pattern

Always provide a custom hook with a guard clause:

```typescript
export function useMyContext() {
  const context = useContext(MyContext);
  if (!context) {
    throw new Error("useMyContext must be used within a MyProvider");
  }
  return context;
}
```

### Reducer Pattern

- Pure function, no side effects.
- Use discriminated union for action types.
- Spread state for immutable updates.

```typescript
export type MyAction =
  | { type: "FETCH_START" }
  | { type: "FETCH_SUCCESS"; items: Item[] }
  | { type: "FETCH_ERROR"; message: string }
  | { type: "SET_FILTER"; filter: string };

export function myReducer(state: MyState, action: MyAction): MyState {
  switch (action.type) {
    case "FETCH_START":
      return { ...state, loading: true, error: null };
    case "FETCH_SUCCESS":
      return { ...state, loading: false, items: action.items };
    case "FETCH_ERROR":
      return { ...state, loading: false, error: action.message };
    case "SET_FILTER":
      return { ...state, filter: action.filter };
  }
}
```

### State Shape Convention

```typescript
export type MyDashboardState = {
  items: Item[];
  filter: string;
  loading: boolean;
  error: string | null;
  notificationCount: number;
};
```

Always include `loading`, `error`, and the primary data array.

---

## Component Patterns

### Presentational Components (Props-Based)

For components that display data passed via props:

```typescript
interface EventCardProps {
  event: Event;
}

export function EventCard({ event }: EventCardProps) {
  return <div className="event-card">{event.title}</div>;
}
```

- Define an explicit `Props` interface. No `any`.
- Destructure props in the function signature.
- Named exports preferred. One component per file.

### Container Components (Context-Based)

Components that read from context do NOT receive state as props:

```typescript
export default function Dashboard() {
  const { state, dispatch, filteredLoans } = useLoanContext();
  // Use state directly — no prop drilling
}
```

### Rule: Do NOT prop-drill context data. Components that need shared state should call the context hook directly.

---

## Forms

### Controlled Inputs with Validation

```typescript
type FormData = {
  name: string;
  amount: number | "";
};

const [formData, setFormData] = useState<FormData>({ name: "", amount: "" });
const [errors, setErrors] = useState<Record<string, string | undefined>>({});
const [touched, setTouched] = useState<Set<string>>(new Set());
```

### Generic Change Handler

```typescript
const handleChange = (
  e: React.ChangeEvent<
    HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement
  >,
) => {
  const { name, value, type } = e.target;
  setFormData((prev) => ({
    ...prev,
    [name]: type === "number" ? (value === "" ? "" : Number(value)) : value,
  }));
  // Validate on change only if field has been touched
  if (touched.has(name)) {
    setErrors((prev) => ({ ...prev, [name]: validateField(name, value) }));
  }
};
```

### Validation

- Validate on blur (mark field as touched).
- Validate all fields on submit.
- Field-level validation function returns `string | undefined`.
- Display errors inline below inputs.

---

## Data Fetching

### API Module Pattern

Define fetch functions in `data/` files alongside type definitions:

```typescript
const API_BASE = "http://localhost:5000";

export async function fetchItems(): Promise<Item[]> {
  const response = await fetch(`${API_BASE}/api/items`);
  if (!response.ok) throw new Error(`API error: ${response.status}`);
  return response.json();
}
```

- Use native `fetch` — no axios or other libraries.
- Always check `response.ok` (fetch does not throw on 4xx/5xx).
- Type the return value explicitly.

### Data Fetching in Context

Fetch on mount using `useEffect` with reducer dispatch:

```typescript
useEffect(() => {
  dispatch({ type: "FETCH_START" });
  fetchItems()
    .then((items) => dispatch({ type: "FETCH_SUCCESS", items }))
    .catch((err) => dispatch({ type: "FETCH_ERROR", message: err.message }));
}, []);
```

---

## Routing

- Use React Router 7 with `BrowserRouter`, `Routes`, `Route`.
- Wrap Provider around Routes so all pages access context.
- Use `Link` or `useNavigate` for navigation.

```typescript
<BrowserRouter>
  <MyProvider>
    <Routes>
      <Route path="/" element={<Dashboard />} />
      <Route path="/create" element={<CreateForm />} />
    </Routes>
  </MyProvider>
</BrowserRouter>
```

---

## Styling

- CSS Modules: `ComponentName.module.css` colocated with the component.
- Import as: `import styles from "./ComponentName.module.css";`
- Usage: `<div className={styles.card}>`
- Conditional classes: ``className={`${styles.card} ${isActive ? styles.active : ""}`}``
- No CSS-in-JS libraries, no Tailwind (unless explicitly added).

---

## TypeScript Conventions

- Strict mode. No `any`.
- Define explicit interfaces for props, state, and API response types.
- Use `type` for unions and simple shapes, `interface` for component props.
- Keep type definitions in `types/` for shared types, colocate with components for local types.
- Use `React.ChangeEvent<HTMLInputElement>` for event handler types.

---

## Quality Checks

- Run `npm run lint` and `npm run build` after changes.
- Handle all three UI states: loading, error, and empty/success.
- Use semantic HTML and ARIA attributes for accessibility.
