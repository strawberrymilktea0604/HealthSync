import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';

interface LineChartCardProps {
  title: string;
  data: Record<string, unknown>[];
  dataKey: string;
  xAxisKey: string;
  color?: string;
}

export function LineChartCard({
  title,
  data,
  dataKey,
  xAxisKey,
  color = '#82ca9d',
}: LineChartCardProps) {
  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h3 className="text-xl font-semibold mb-4">{title}</h3>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey={xAxisKey} />
          <YAxis />
          <Tooltip />
          <Legend />
          <Line type="monotone" dataKey={dataKey} stroke={color} strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
