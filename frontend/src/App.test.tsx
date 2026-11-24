import { describe, expect, it, vi } from 'vitest';
import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import App from './App';

interface Todo {
  id: number;
  title: string;
  completed: boolean;
  createdAt: string;
}

const json = (body: unknown, status = 200) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json' } });

/** Simula la API de /api/todos en memoria, sustituyendo fetch. */
function mockApi(initial: Todo[] = []) {
  const todos = [...initial];
  let nextId = 100;

  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = input.toString();
    const method = init?.method ?? 'GET';

    if (url === '/api/todos' && method === 'GET') return json(todos);

    if (url === '/api/todos' && method === 'POST') {
      const { title } = JSON.parse(init!.body as string);
      const todo = { id: nextId++, title, completed: false, createdAt: new Date().toISOString() };
      todos.push(todo);
      return json(todo, 201);
    }

    const match = url.match(/^\/api\/todos\/(\d+)$/);
    const index = match ? todos.findIndex((t) => t.id === Number(match[1])) : -1;
    if (index === -1) return json({}, 404);

    if (method === 'PUT') {
      Object.assign(todos[index], JSON.parse(init!.body as string));
      return json(todos[index]);
    }
    if (method === 'DELETE') {
      todos.splice(index, 1);
      return json({});
    }
    return json({}, 405);
  });

  vi.stubGlobal('fetch', fetchMock);
  return fetchMock;
}

const todo = (id: number, title: string, completed = false): Todo => ({
  id,
  title,
  completed,
  createdAt: '2026-11-24T10:00:00Z',
});

describe('App', () => {
  it('muestra un mensaje cuando no hay tareas', async () => {
    mockApi();
    render(<App />);

    expect(await screen.findByText('Nothing here yet. Add your first task above.')).toBeInTheDocument();
    expect(screen.getByText('0 tasks left')).toBeInTheDocument();
  });

  it('muestra las tareas que devuelve la API y cuenta las pendientes', async () => {
    mockApi([todo(1, 'Escribir el capítulo 4'), todo(2, 'Crear el repositorio', true)]);
    render(<App />);

    expect(await screen.findByText('Escribir el capítulo 4')).toBeInTheDocument();
    expect(screen.getByText('Crear el repositorio')).toBeInTheDocument();
    expect(screen.getByText('1 task left')).toBeInTheDocument();
    expect(screen.getByText('1 of 2 done')).toBeInTheDocument();
  });

  it('añade una tarea nueva mediante POST', async () => {
    const fetchMock = mockApi();
    const user = userEvent.setup();
    render(<App />);

    await user.type(screen.getByPlaceholderText('What needs to be done?'), 'Configurar CodeQL');
    await user.click(screen.getByRole('button', { name: 'Add task' }));

    expect(await screen.findByText('Configurar CodeQL')).toBeInTheDocument();
    expect(fetchMock).toHaveBeenCalledWith('/api/todos', expect.objectContaining({ method: 'POST' }));
    expect(screen.getByPlaceholderText('What needs to be done?')).toHaveValue('');
  });

  it('no permite añadir una tarea vacía', async () => {
    mockApi();
    const user = userEvent.setup();
    render(<App />);

    await user.type(screen.getByPlaceholderText('What needs to be done?'), '   ');

    expect(screen.getByRole('button', { name: 'Add task' })).toBeDisabled();
  });

  it('marca una tarea como completada', async () => {
    mockApi([todo(1, 'Revisar los tests')]);
    const user = userEvent.setup();
    render(<App />);

    const checkbox = await screen.findByRole('checkbox', { name: /Revisar los tests/ });
    await user.click(checkbox);

    expect(await screen.findByText('0 tasks left')).toBeInTheDocument();
    expect(screen.getByRole('checkbox', { name: /Revisar los tests/ })).toBeChecked();
  });

  it('filtra las tareas completadas', async () => {
    mockApi([todo(1, 'Pendiente'), todo(2, 'Hecha', true)]);
    const user = userEvent.setup();
    render(<App />);

    await screen.findByText('Pendiente');
    await user.click(screen.getByRole('button', { name: 'Done' }));

    const list = screen.getByRole('list');
    expect(within(list).getByText('Hecha')).toBeInTheDocument();
    expect(within(list).queryByText('Pendiente')).not.toBeInTheDocument();
  });

  it('elimina una tarea', async () => {
    mockApi([todo(1, 'Tarea a borrar')]);
    const user = userEvent.setup();
    render(<App />);

    await user.click(await screen.findByRole('button', { name: /Delete .*Tarea a borrar/ }));

    expect(await screen.findByText('Nothing here yet. Add your first task above.')).toBeInTheDocument();
  });
});
