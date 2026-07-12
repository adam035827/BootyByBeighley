import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  OnInit,
  signal,
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { TodoItemsClient, TodoItemDto } from '../../../../core/api/api';
import { ErrorService } from '../../../../core/services/error.service';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './todo-list.component.html',
  styleUrl: './todo-list.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TodoListComponent implements OnInit {
  private readonly client = inject(TodoItemsClient);
  private readonly errorService = inject(ErrorService);

  readonly items = signal<TodoItemDto[]>([]);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly pendingItemCount = computed(() => this.items().filter(i => !i.isCompleted).length);

  ngOnInit(): void {
    this.loadItems();
  }

  loadItems(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.client.getTodoItems().subscribe({
      next: items => {
        this.items.set(items);
        this.isLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(this.errorService.extractMessage(err));
        this.isLoading.set(false);
      },
    });
  }

  toggleComplete(item: TodoItemDto): void {
    this.client
      .updateTodoItem(item.id, {
        title: item.title,
        description: item.description,
        isCompleted: !item.isCompleted,
      })
      .subscribe({
        next: updated => {
          this.items.update(items => items.map(i => (i.id === updated.id ? updated : i)));
        },
        error: (err: HttpErrorResponse) => {
          this.errorMessage.set(this.errorService.extractMessage(err));
        },
      });
  }

  delete(id: string): void {
    this.client.deleteTodoItem(id).subscribe({
      next: () => {
        this.items.update(items => items.filter(i => i.id !== id));
      },
      error: (err: HttpErrorResponse) => {
        this.errorMessage.set(this.errorService.extractMessage(err));
      },
    });
  }
}

